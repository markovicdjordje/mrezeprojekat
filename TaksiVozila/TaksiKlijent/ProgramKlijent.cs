using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TaxiCore.Models;
using TaxiCore.Enums;
using System.Security.Cryptography;

namespace TaksiKlijent
{
    class ProgramKlijent
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
                while (true)
                {
                    udpClient = new UdpClient();
                    IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(SERVER_IP), SERVER_PORT);

                    // Unos podataka od korisnika
                    Console.Write("Unesite početne koordinate (X) ili 'exit' za kraj: ");
                    string pocetnaTackaX = Console.ReadLine();
                    if (pocetnaTackaX.ToLower() == "exit")
                        break;
                    int pocetnaX;
                    if (int.TryParse(pocetnaTackaX, out pocetnaX) == false)
                    {
                        Console.WriteLine("UNESI BROJ");
                        return;
                    }


                    Console.Write("Unesite početne koordinate (Y): ");
                    string pocetnaTackaY = Console.ReadLine();
                    int pocetnaY;
                    if (int.TryParse(pocetnaTackaY, out pocetnaY) == false)
                    {
                        Console.WriteLine("UNESI BROJ");
                        return;
                    }

                    Console.Write("Unesite krajnje koordinate (X): ");
                    string krajnjaTackaX = Console.ReadLine();
                    int krajnjaX;
                    if (int.TryParse(krajnjaTackaX, out krajnjaX) == false)
                    {
                        Console.WriteLine("UNESI BROJ");
                        return;
                    }

                    Console.Write("Unesite krajnje koordinate (Y): ");
                    string krajnjaTackaY = Console.ReadLine();
                    int krajnjaY;
                    if (int.TryParse(krajnjaTackaY, out krajnjaY) == false)
                    {
                        Console.WriteLine("UNESI BROJ");
                        return;
                    }

                    Klijent klijent = new Klijent
                    {
                        PocetneKoordinate = new Koordinate(pocetnaX, pocetnaY),
                        KrajnjeKoordinate = new Koordinate(krajnjaX, krajnjaY),
                        StatusKlijenta = StatusKlijenta.Cekanje
                    };

                    // Kreiraj zahtev
                    // izmena ovde string zahtev = $"Od: {pocetnaX}:{pocetnaY} -> Do: {krajnjaX}:{krajnjaY}";
                    string zahtev = $"{pocetnaX}:{pocetnaY}:{krajnjaX}:{krajnjaY}";
                    byte[] zahtevBytes = Encoding.UTF8.GetBytes(zahtev);

                    // Pošalji zahtev serveru preko UDP
                    udpClient.Send(zahtevBytes, zahtevBytes.Length, serverEndPoint);
                    Console.WriteLine("\n✓ Zahtev poslat serveru!\n");

                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] odgovorBytes = udpClient.Receive(ref remoteEP);
                    string odgovor = Encoding.UTF8.GetString(odgovorBytes);

                    Console.WriteLine($"Server odgovara: {odgovor}");
                }
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
