using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TaksiVozila
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SERVER POKRENUT");

            // TCP za taksi vozila
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 5000);
            tcpListener.Start();
            Console.WriteLine("TCP listener (vozila) na portu 5000");














        }
    }
}
