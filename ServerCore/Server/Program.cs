using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using static System.Collections.Specialized.BitVector32;


namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            Console.WriteLine("Server Start");

            //로컬호스트의 도메인 획득
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            //어떤 종류의 세션을 만들어줄지 지정 => 여기서는 GameSession
            _listener.Init(endPoint, () => { return new ClientSession(); });
            Console.WriteLine(" Listening..");

            while (true)
            {

            }
        }
    }
}