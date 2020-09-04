using System;

namespace SocketAsync
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Socket服务端启动，端口号:30011");
            var key =  Console.ReadLine();
            if (key.Equals("s"))
            {
                SocketServer server = new SocketServer();
                server.StartListen();
            }
            else
            {
                SocketClient client = new SocketClient();
                client.Connect();
            }
            Console.ReadLine();
        }
    }
}
