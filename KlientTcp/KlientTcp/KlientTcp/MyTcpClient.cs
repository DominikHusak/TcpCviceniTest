using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KlientTcp
{
    public class MyTcpClient
    {
        private TcpClient myClient;
        private bool isRunning;

        public MyTcpClient(string ipAddress, int port)
        {
            myClient = new TcpClient(ipAddress, port);
            isRunning = true;
            ClientLoop();

        }

        private void ClientLoop()
        {
            try
            {
                Console.WriteLine("Pripojili jste se na server");
                StreamReader reader = new StreamReader(myClient.GetStream(), Encoding.UTF8);
                StreamWriter writer = new StreamWriter(myClient.GetStream(), Encoding.UTF8);
                string? data = null;
                string? dataRecive = null;
                while (isRunning)
                {
                    Console.Write(">> ");
                    dataRecive = Console.ReadLine();
                    writer.WriteLine(dataRecive);
                    writer.Flush();
                    if (dataRecive == "end")
                    {
                        isRunning = false;
                    }
                    data = reader.ReadLine();
                    //data = data.ToLower();

                    Console.WriteLine(data);
                }
                writer.WriteLine("Byl jste uspesne odpojen");
                writer.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
    }
}
