using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TaxiVoziloApp
{
    class Program
    {
        private static TcpClient tcpClient;
        private static NetworkStream stream;
        private const string SERVER_IP = "127.0.0.1"; // localhost
        private const int SERVER_PORT = 5000;

        static void Main(string[] args)
        {
            Console.WriteLine("=== TAKSI VOZILO ===");
            Console.WriteLine("Povezivanje sa serverom...\n");

            try
            {
                // Poveži se sa serverom preko TCP
                tcpClient = new TcpClient(SERVER_IP, SERVER_PORT);
                stream = tcpClient.GetStream();
                Console.WriteLine("✓ Povezano sa serverom!\n");

                // Pošalji početne podatke vozilu
                Console.Write("Unesite trenutnu poziciju (npr. X:10, Y:20): ");
                string pozicija = Console.ReadLine();

                Console.Write("Unesite status (slobodan/zauzet): ");
                string status = Console.ReadLine();

                string podaciVozila = $"Pozicija: {pozicija}, Status: {status}";
                byte[] podatci = Encoding.UTF8.GetBytes(podaciVozila);
                stream.Write(podatci, 0, podatci.Length);
                Console.WriteLine("\n✓ Podaci poslati serveru!\n");

                // Primi potvrdu od servera
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string potvrda = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Server kaže: {potvrda}\n");

                // Sluša zadatke od servera
                Console.WriteLine("Čekam zadatke od servera...\n");
                while (true)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string zadatak = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"\n🚖 PRIMLJEN ZADATAK:");
                        Console.WriteLine($"   {zadatak}");
                        Console.WriteLine("\n[Vozilo kreće na lokaciju...]\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GREŠKA: {ex.Message}");
            }
            finally
            {
                stream?.Close();
                tcpClient?.Close();
            }
        }
    }
}
