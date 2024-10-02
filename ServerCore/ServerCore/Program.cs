﻿using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    internal class Program
    {
        static Listener _listener = new Listener();

        // 콜백함수로 등록할 함수
        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                // 세션 단위로 관리
                Session session = new Session();
                session.Start(clientSocket);

                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
                session.Send(sendBuff);

                Thread.Sleep(1000);

                session.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Server Start");

            //로컬호스트의 도메인 획득
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, OnAcceptHandler);
            Console.WriteLine(" Listening..");

            while (true)
            {

            }
        }
    }
}
