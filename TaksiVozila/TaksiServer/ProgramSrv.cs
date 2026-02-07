using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaxiCore.Models;
using TaxiCore.Enums;

namespace TaksiServer
{
    class ProgramSrv
    {
        private static TcpListener tcpListener;
        private static UdpClient udpClient;

        private const int TCP_PORT = 5000; // Port za vozila
        private const int UDP_PORT = 5001; // Port za klijente

        private static List<TaksiVozilo> TaksiVozila = new List<TaksiVozilo>();
        private static List<Klijent> Klijenti = new List<Klijent>();
        private static readonly object lockObj = new object();

        //private static string statusVozila = null;

        static void Main(string[] args)
        {
            Console.WriteLine("=== TAKSI SERVER ===");
            Console.WriteLine("Pokretanje servera...\n");

            // Pokreni TCP listener za vozila
            tcpListener = new TcpListener(IPAddress.Any, TCP_PORT);
            tcpListener.Start();
            Console.WriteLine($"[TCP] Server pokrenut na portu {TCP_PORT} - čeka vozila...");

            // Pokreni UDP soket za klijente
            udpClient = new UdpClient(UDP_PORT);
            Console.WriteLine($"[UDP] Server pokrenut na portu {UDP_PORT} - čeka klijente...\n");

            // Pokreni thread za prihvatanje vozila
            Thread tcpThread = new Thread(PrihvatiVozilo);
            tcpThread.Start();

            Thread visuelizacijaThread = new Thread(VizuelizacijaLoop);
            visuelizacijaThread.Start();

            // Glavna petlja za UDP zahteve od klijenata
            PrihvatiKlijente();

        }

        // TCP - Prihvati povezivanje vozila
        static void PrihvatiVozilo()
        {
            while (true)
            {
                try
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    NetworkStream stream = tcpClient.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string podaciVozila = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var delovi = podaciVozila.Split(',');
                    int x = int.Parse(delovi[0]);
                    int y = int.Parse(delovi[1]);

                    TaksiVozilo taksiVozilo = new TaksiVozilo
                    {
                        TcpClient = tcpClient,
                        Stream = stream,
                        KoordinateVozila = new Koordinate(x, y),
                        StatusVozila = StatusVozila.Slobodno,
                        Zarada = 0,
                        PredjenaKilometraza = 0
                    };

                    lock (lockObj)
                    {
                        TaksiVozila.Add(taksiVozilo);
                    }

                    Console.Write($"[TCP] Novo vozilo se povezalo: {podaciVozila}");

                    string potvrda = "Server: Vozilo registrovano";
                    byte[] potvrdaBytes = Encoding.UTF8.GetBytes(potvrda);
                    stream.Write(potvrdaBytes, 0, potvrdaBytes.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GREŠKA TCP] {ex.Message}");
                }
            }
        }

        // UDP - Prihvati zahteve od klijenata
        static void PrihvatiKlijente()
        {
            IPEndPoint klijentEP = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    byte[] primljeniPodaci = udpClient.Receive(ref klijentEP);
                    string zahtev = Encoding.UTF8.GetString(primljeniPodaci);

                    Klijent klijent = new Klijent
                    {
                        PocetneKoordinate = new Koordinate(1, 1),
                        KrajnjeKoordinate = new Koordinate(2, 2),
                        StatusKlijenta = StatusKlijenta.Cekanje
                    };

                    lock (lockObj)
                    {
                        Klijenti.Add(klijent);
                    }

                    Console.WriteLine($"[UDP] Zahtev od klijenta {klijentEP}: {zahtev}");

                    TaksiVozilo slobodnoVozilo = null;

                    lock (lockObj)
                    {
                        slobodnoVozilo = TaksiVozila.FirstOrDefault(v => v.StatusVozila == StatusVozila.Slobodno);

                        if (slobodnoVozilo != null)
                        {
                            slobodnoVozilo.StatusVozila = StatusVozila.Odlazak_Na_Lokaciju;
                            klijent.StatusKlijenta = StatusKlijenta.Prihvaceno;
                        }
                    }

                    if (slobodnoVozilo != null)
                    {
                        string zadatak = $"NOVI ZADATAK: {klijent.PocetneKoordinate.X}, {klijent.PocetneKoordinate.Y} - > {klijent.KrajnjeKoordinate.X}, {klijent.KrajnjeKoordinate.Y}";

                        byte[] zadatakBytes = Encoding.UTF8.GetBytes(zadatak);
                        slobodnoVozilo.Stream.Write(zadatakBytes, 0, zadatakBytes.Length);

                        string odgovor = "Server: Vas zahtev je prihvacen. Vozilo je na putu!";
                        byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                        udpClient.Send(odgovorBytes, odgovorBytes.Length, klijentEP);

                        Console.WriteLine($"[SERVER] Zadovoljeno: zadatak poslat vozilu, klijent obavesten.");
                    }
                    else
                    {
                        string odgovor = "Server: Trenutno nema slobodnih vozila";
                        byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                        udpClient.Send(odgovorBytes, odgovorBytes.Length, klijentEP);

                        Console.WriteLine($"[UPOZORENJE] Nema slobodnih vozila!");
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"[GREŠKA UDP] {ex.Message}");
                }
            }
        }

        static void VizuelizacijaLoop()
        {
            while (true)
            {
                Thread.Sleep(2000);
                //Console.Clear();
                lock (lockObj)
                {
                    Console.WriteLine("=== STATUS VOZILA ===");
                    Console.WriteLine("ID\tPozicijaX\tPozicijaY\tStatus\tKm\tZarada");
                    int id = 1;
                    foreach (var vozilo in TaksiVozila)
                    {
                        Console.WriteLine($"{id++}\t" +
                            $"{vozilo.KoordinateVozila.X}\t" +
                            $"{vozilo.KoordinateVozila.Y}\t" +
                            $"{vozilo.StatusVozila}\t" +
                            $"{vozilo.PredjenaKilometraza}\t" +
                            $"{vozilo.Zarada}");
                    }

                    Console.WriteLine("\n=== AKTIVNI KLIJENTI ===");
                    Console.WriteLine("ID\tPocetnaX\tPocetnaY\tKrajnjaX\tKrajnjaY\tStatus");
                    id = 1;

                    foreach (var k in Klijenti)
                    {
                        Console.WriteLine($"\n{id++}\t{k.PocetneKoordinate.X}\t" +
                            $"{k.PocetneKoordinate.Y}\t" +
                            $"{k.KrajnjeKoordinate.X}\t" +
                            $"{k.KrajnjeKoordinate.Y}\t" +
                            $"{k.StatusKlijenta}");
                    }
                }
            }
        }
    }
}
