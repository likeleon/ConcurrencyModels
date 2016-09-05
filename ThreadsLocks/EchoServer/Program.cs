using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EchoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
            var listener = new TcpListener(ipAddress, 4567);
            listener.Start();
            try
            {
                while (true)
                {
                    var socket = listener.AcceptSocket();
                    var thread = new Thread(HandleConnection);
                    thread.Start(socket);
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        static void HandleConnection(object o)
        {
            var socket = o as Socket;
            if (socket == null)
                throw new ArgumentNullException(nameof(o));

            try
            {
                using (var stream = new NetworkStream(socket, true))
                {
                    var inStream = new StreamReader(stream);
                    var outStream = new StreamWriter(stream);

                    var bytesRead = 0;
                    var buffer = new char[1024];
                    while ((bytesRead = inStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outStream.Write(buffer, 0, bytesRead);
                        outStream.Flush();
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
