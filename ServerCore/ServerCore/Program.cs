using System.Net;
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
                //데이터를 받는다
                byte[] recvBuff = new byte[1024];
                int recvBytes = clientSocket.Receive(recvBuff);
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                Console.WriteLine($"[From client] {recvData}");

                //데이터를 보낸다
                byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
                clientSocket.Send(sendBuff);

                //쫓아낸다(종료).
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
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
