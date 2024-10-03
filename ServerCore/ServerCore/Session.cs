using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class Session
    {
        Socket _socket;
        int _disconnected = 0; // 스레드 세이프 확인용 변수

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        // Args 전역변수로 선언하여 재사용
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
        {
            _socket = socket;

            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv(recvArgs);
        }

        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendingList.Count == 0)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            //쫓아낸다(종료).
            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        #region 네트워크 통신
        void RegisterSend()
        {
            while (_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }

            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        // 호출되는 경우가 두 가지 - 다른 비동기 함수들과 같음.
        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        try
                        {
                            _sendArgs.BufferList = null;
                            _pendingList.Clear();

                            OnSend(_sendArgs.BytesTransferred);

                            if (_sendQueue.Count > 0)
                                RegisterSend();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"OnSendCompleted Failed {e}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);
            if (pending == false)
                OnRecvCompleted(null, args);
        }

        // 호출되는 경우가 두 가지
        // 1. _socket.ReceiveAsync(args)가 true여서 나중에 알아서 이벤트로 호출됨
        // 2. _socket.ReceiveAsync(args)가 false(동기적으로 완료) 이면 이벤트핸들러(OnAcceptCompleted)가 호출되지 않아 '직접' 호출해야함
        // recvArgs.Completed 콜백은 작업이 비동기적으로 완료될 때만 호출
        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                //TODO
                try
                {
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
                    // 정상적으로 Recv 되었으면 재등록
                    RegisterRecv(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e}");
                }
            }
            else
            {
                OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));
            }
        }
        #endregion 네트워크 통신
    }
}
