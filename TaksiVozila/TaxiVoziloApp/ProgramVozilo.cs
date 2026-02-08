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
        private const string SERVER_IP = "127.0.0.1";
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

                    TcpClient tcpClient = new TcpClient(SERVER_IP, SERVER_PORT);
                    NetworkStream stream = tcpClient.GetStream();

                    string registracija = $"{x},{y}";
                    byte[] data = Encoding.UTF8.GetBytes(registracija);
                    stream.Write(data, 0, data.Length);

                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    Console.WriteLine($"Server: {Encoding.UTF8.GetString(buffer, 0, bytesRead)}");

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
