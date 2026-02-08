/*using System;
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
}*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TaxiCore.Enums;
using TaxiCore.Models;

namespace TaxiVoziloApp
{
    class ProgramVozilo
    {
        private const string SERVER_IP = "127.0.0.1"; // localhost
        private const int SERVER_PORT = 5000;

        static void Main(string[] args)
        {
            Console.WriteLine("=== TAKSI VOZILO ===");
            Console.WriteLine("Povezivanje sa serverom...\n");

            try
            {
                Console.Write("Koliko vozila želite da registrujete? ");
                int brojVozila = int.Parse(Console.ReadLine());

                for (int i = 0; i < brojVozila; i++)
                {
                    Console.WriteLine($"\nRegistracija vozila #{i + 1}");

                    Console.Write("Unesite X: ");
                    int x = int.Parse(Console.ReadLine());

                    Console.Write("Unesite Y: ");
                    int y = int.Parse(Console.ReadLine());

                    // Kreiraj TCP konekciju za ovo vozilo
                    TcpClient tcpClient = new TcpClient(SERVER_IP, SERVER_PORT);
                    NetworkStream stream = tcpClient.GetStream();

                    // Pošalji registraciju
                    string registracija = $"{x},{y}";
                    byte[] data = Encoding.UTF8.GetBytes(registracija);
                    stream.Write(data, 0, data.Length);

                    // Čekaj potvrdu
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    Console.WriteLine($"Server: {Encoding.UTF8.GetString(buffer, 0, bytesRead)}");

                    // Pokreni thread za ovo vozilo
                    Thread t = new Thread(() => ObradaVozila(tcpClient, stream, i));
                    t.Start();
                }

                Console.WriteLine("\nSva vozila su registrovana. Server čeka zadatke...");
                Console.WriteLine("Pritisnite ENTER za izlaz iz aplikacije...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GREŠKA: {ex.Message}");
            }
        }

        static void ObradaVozila(TcpClient tcpClient, NetworkStream stream, int voziloId)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine($"[VOZILO {voziloId}] Konekcija prekinuta.");
                        break;
                    }

                    string poruka = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (!poruka.StartsWith("NOVI_ZADATAK"))
                        continue;

                    Console.WriteLine($"[VOZILO {voziloId}] Primljen zadatak: {poruka}");

                    Console.WriteLine($"[VOZILO {voziloId}] Idem po klijenta...");
                    Thread.Sleep(3000);
                    Posalji(stream, "STIGAO_PO_KLIJENTA");

                    Console.WriteLine($"[VOZILO {voziloId}] Vozim klijenta...");
                    Thread.Sleep(5000);
                    Posalji(stream, "VOZNJA_ZAVRSENA");

                    Console.WriteLine($"[VOZILO {voziloId}] Voznja zavrsena\n");
                }
            }
            catch
            {
                Console.WriteLine($"[VOZILO {voziloId}] Došlo je do greške ili diskonektovanja.");
            }
            finally
            {
                stream?.Close();
                tcpClient?.Close();
            }
        }

        static void Posalji(NetworkStream stream, string poruka)
        {
            byte[] data = Encoding.UTF8.GetBytes(poruka);
            stream.Write(data, 0, data.Length);
        }
    }
}
