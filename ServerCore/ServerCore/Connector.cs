using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> _sessionFactory;

        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            //휴대폰 설정
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory = sessionFactory;

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;

            RegisterConnect(args);
        }

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket sokcet = args.UserToken as Socket;
            if (sokcet == null)
                return;
            bool pending = sokcet.ConnectAsync(args);
            if (pending == false)
                OnConnectCompleted(null, args);
        }

        // 호출되는 경우가 두 가지 - 다른 비동기 함수들과 같음.
        void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectCompleted Fail : {args.SocketError}");
            }
        }
    }
}
