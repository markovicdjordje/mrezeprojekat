using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

                string registracija = $"{pocetnaX}, {pocetnaY}, {StatusVozila.Slobodno}";
                Posalji(registracija);

                string potvrda = Procitaj();
                Console.WriteLine($"Server: {potvrda}");
                Console.WriteLine("Cekam zadatke...\n");

                while (true)
                {
                    string poruka = Procitaj();
                    //if (poruka == null) break;

                    if (poruka == null)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    if (!poruka.StartsWith("NOVI_ZADATAK"))
                        continue;

                    Console.WriteLine($"[ZADATAK] {poruka}");

                    Console.WriteLine("Idem po klijenta...");
                    Thread.Sleep(3000);
                    Posalji("STIGAO_PO_KLIJENTA");

                    Console.WriteLine("Vozim klijenta...");
                    Thread.Sleep(5000);
                    Posalji("VOZNJA_ZAVRSENA");

                    Console.WriteLine("Voznja zavrsena\n");
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
        static void Posalji(string poruka)
        {
            byte[] data = Encoding.UTF8.GetBytes(poruka);
            stream.Write(data, 0, data.Length);
        }

        static string Procitaj()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0) return null;

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
    }
}
