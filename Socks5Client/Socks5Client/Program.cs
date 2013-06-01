using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Socks5Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "www.google.com";
            Socket socket = Socks5Client.Connect("localhost", 8081, host, 80, null, null);
            string userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:21.0) Gecko/20100101 Firefox/21.0";
            byte[] request = Encoding.ASCII.GetBytes(String.Format("GET / HTTP/1.1\r\nHost: {0}\r\nUser-Agent: {1}\r\n\r\n", host, userAgent));
            socket.Send(request);
            byte[] buffer = new byte[2048];
            int recv;
            while ((recv = socket.Receive(buffer, 2048, SocketFlags.None)) > 0)
            {
                string response = Encoding.ASCII.GetString(buffer, 0, recv);
                Console.Write(response);
                if (!socket.Poll(1000 * 1000, SelectMode.SelectRead))
                    break;
            }
            socket.Close();
            Console.ReadLine();
        }
    }
}
