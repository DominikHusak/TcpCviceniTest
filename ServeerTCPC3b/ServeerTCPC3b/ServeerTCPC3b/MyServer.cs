using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServeerTCPC3b
{
    public class MyServer
    {
        private TcpListener myServer;
        private bool isRunning;

        public MyServer(int port)
        {
            myServer = new TcpListener(System.Net.IPAddress.Any, port);
            myServer.Start();
            isRunning = true;
            ServerLoop();
        }

        private void ServerLoop()
        {
            Console.WriteLine("Server byl spusten");
            while (isRunning)
            {
                TcpClient client = myServer.AcceptTcpClient();
                ClientLoop(client);
            }
        }

        private void ClientLoop(TcpClient client)
        {
            StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8);
            StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.UTF8);

            writer.WriteLine("Byl jsi pripojen");
            writer.Flush();
            bool clientConnect = true;
            string? data = null;
            string? dataRecive = null;
            while (clientConnect)
            {
                data = reader.ReadLine();
                data = data.ToLower();
                if (data == "end")
                {
                    clientConnect = false;
                }

                dataRecive = data + " prijato";
                writer.WriteLine(dataRecive);
                writer.Flush();
            }
            writer.WriteLine("Byl jsi odpojen");
            writer.Flush();
        }
    }
}
