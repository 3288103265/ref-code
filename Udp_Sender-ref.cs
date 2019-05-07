using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

//验证，可否使用socket连接到uwp程序，如果不行的话，还是要写uwp版的发送版。
namespace Sender
{
    class Program
    {
        static void Main(string[] args)
        {
            string remoteIP = "192.168.1.147";
            long remotePort = 8080;

            Thread sendThread = new Thread(new ThreadStart(SendService));
            sendThread.Start();

            void SendService()
            {
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
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
                //流式文件可以提前知道文件长度，如果可以的话，到时候可以更加精细化接收
                byte[] imgbyte = new byte[file.Length];
                file.Read(imgbyte, 0, imgbyte.Length);
                file.Close();
                return imgbyte;
            }
        }
    }
}
