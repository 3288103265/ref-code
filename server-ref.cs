using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket server = new Socket(SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint thisEP = new IPEndPoint(IPAddress.Parse("210.72.22.237"), 11000);
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remoteHost = remoteEP;

            server.Bind(thisEP);

            byte[] recvBytes = new byte[65535];
            int count = 0;
            while (true)
            {
                try
                {
                    server.ReceiveFrom(recvBytes, ref remoteHost);
                    var recvImg = Byte2Img(recvBytes);
                    count++;

                    recvImg.Save("E:\\result\\" + count.ToString()+".jpg", ImageFormat.Jpeg);
                    Console.WriteLine("Saved {0}th image.", count);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            Image Byte2Img(byte[] imgByte)
            {
                //Convert Bytestream to Image.
                var ms = new MemoryStream(imgByte);
                return Bitmap.FromStream(ms, true);
            }
        }
    }
}
