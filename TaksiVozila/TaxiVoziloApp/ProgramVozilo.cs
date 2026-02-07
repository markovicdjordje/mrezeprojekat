using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TaxiCore.Enums;
using TaxiCore.Models;

namespace TaxiVoziloApp
{
    class ProgramVozilo
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
                Console.Write("Unesite koordinate vozila (X) ");
                string pocetnaTackaX = Console.ReadLine();
                int pocetnaX;
                if (int.TryParse(pocetnaTackaX, out pocetnaX) == false)
                {
                    Console.WriteLine("UNESI BROJ");
                    return;
                }

                Console.Write("Unesite koordinate vozila (Y) ");
                string pocetnaTackaY = Console.ReadLine();
                int pocetnaY;
                if (int.TryParse(pocetnaTackaY, out pocetnaY) == false)
                {
                    Console.WriteLine("UNESI BROJ");
                    return;
                }

                TaksiVozilo taksi = new TaksiVozilo
                {
                    KoordinateVozila = new Koordinate(pocetnaX, pocetnaY),
                    StatusVozila = StatusVozila.Slobodno
                };
                taksi.TcpClient = tcpClient;
                taksi.Stream = stream;

                string podaciVozila = $"{pocetnaX},{pocetnaY},{StatusVozila.Slobodno}";
                byte[] podatciBytes = Encoding.UTF8.GetBytes(podaciVozila);
                stream.Write(podatciBytes, 0, podatciBytes.Length);
                //stream.Write(taksi.Serialize());
                Console.WriteLine("\n✓ Podaci poslati serveru!\n");

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string potvrda = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Server: {potvrda}\n");

                Console.WriteLine("Cekam zadatke od servera...\n");
                while (true)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string zadatak = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"\nPRIMLJEN ZADATAK: {zadatak}");
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
