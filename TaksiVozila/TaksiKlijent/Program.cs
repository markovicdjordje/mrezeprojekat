using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TaksiKlijent
{
    class Program
    {
        private static UdpClient udpClient;
        private const string SERVER_IP = "127.0.0.1"; // localhost
        private const int SERVER_PORT = 5001;

        static void Main(string[] args)
        {
            Console.WriteLine("=== TAKSI KLIJENT ===");
            Console.WriteLine("Povezivanje sa serverom...\n");

            try
            {
                udpClient = new UdpClient();
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(SERVER_IP), SERVER_PORT);

                // Unos podataka od korisnika
                Console.Write("Unesite početnu tačku (npr. Trg Republike): ");
                string pocetnaTacka = Console.ReadLine();

                Console.Write("Unesite krajnju tačku (npr. Aerodrom): ");
                string krajnjaTacka = Console.ReadLine();

                // Kreiraj zahtev
                string zahtev = $"Od: {pocetnaTacka} -> Do: {krajnjaTacka}";
                byte[] zahtevBytes = Encoding.UTF8.GetBytes(zahtev);

                // Pošalji zahtev serveru preko UDP
                udpClient.Send(zahtevBytes, zahtevBytes.Length, serverEndPoint);
                Console.WriteLine("\n✓ Zahtev poslat serveru!\n");

                Console.WriteLine("Čekam odgovor...\n");

                // Primi odgovor od servera
                IPEndPoint primaocEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] odgovorBytes = udpClient.Receive(ref primaocEndPoint);
                string odgovor = Encoding.UTF8.GetString(odgovorBytes);

                Console.WriteLine($"✓ Server odgovorio:");
                Console.WriteLine($"  {odgovor}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GREŠKA: {ex.Message}");
            }
            finally
            {
                udpClient?.Close();
                Console.WriteLine("\nPritisnite bilo koji taster za izlaz...");
                Console.ReadKey();
            }
        }
    }
}
