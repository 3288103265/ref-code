using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sender
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread sendThread = new Thread(new ThreadStart(SendService));
            sendThread.Start();

            void SendService()
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse("210.72.22.237"), 11000);
                Socket sender = new Socket(SocketType.Dgram, ProtocolType.Udp);

                string addr = "D:\\PythonProject\\Video2frame\\result\\";
                for (int i = 1; i < 156; i++)
                {
                    string imgAddr = addr + i.ToString() + ".jpg";
                    var sendBytes = Img2Bytes(imgAddr);

                    try
                    {
                        sender.SendTo(sendBytes, remoteEP);
                        Console.WriteLine("Send {0}th image.", i);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    Thread.Sleep(40);
                }
                Console.WriteLine("Transmission completed.");
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }

            byte[] Img2Bytes(string path)
            {
                var file = new FileStream(path, FileMode.Open);
                byte[] imgbyte = new byte[file.Length];
                file.Read(imgbyte, 0, imgbyte.Length);
                file.Close();
                return imgbyte;
            }
        }
    }
}
