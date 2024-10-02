using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Listener
    {
        Socket _listenSocket;
        Action<Socket> _onAcceptHandler; //Accept 완로 후 행동

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _onAcceptHandler += onAcceptHandler;

            _listenSocket.Bind(endPoint);

            // 최대 대기 수(백로그) N
            _listenSocket.Listen(10);

            // 연결 성공 시 콜백함수 등록
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

            RegisterAccept(args);
        }


        // 이 부분은 스레드 세이프한가?
        // 세이프하지 않음, 후에 조치
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            //남아 있는 이전 연결을 초기화
            args.AcceptSocket = null;

            // 동기 -> 비동기 (Accept)
            bool pending = _listenSocket.AcceptAsync(args);

            //true : 보류 / false : 동기적으로 완료(바로 완료된 경우)
            if (pending == false)
                OnAcceptCompleted(null, args);
        }

        // 호출되는 경우가 두 가지
        // 1. _listenSocket.AcceptAsync(args)가 true여서 나중에 알아서 이벤트로 호출됨
        // 2. _listenSocket.AcceptAsync(args)가 false(동기적으로 완료) 이면 이벤트핸들러(OnAcceptCompleted)가 호출되지 않아 '직접' 호출해야함
        // args.Completed 콜백은 작업이 비동기적으로 완료될 때만 호출
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                // Accept 완료 후 행동
                // 리스너를 호출하는 부분의 함수 인자(Action)가 여기까지 왔음
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            //등록 완료 후 다음 등록 대기
            RegisterAccept(args);
        }
    }
}
