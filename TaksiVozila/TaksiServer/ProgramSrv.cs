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
using System.IO;

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
        private static List<Zadatak> Zadaci = new List<Zadatak>();
        private static readonly object lockObj = new object();

        //private static int rbTaxi = 0;

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

                        // provera da li postoji klijent koji vec ceka:

                        /*var cekajuciKlijent = Klijenti.FirstOrDefault(k => k.StatusKlijenta == StatusKlijenta.Cekanje);

                        if (cekajuciKlijent != null)
                        {
                            taksiVozilo.StatusVozila = StatusVozila.Odlazak_Na_Lokaciju;
                            cekajuciKlijent.StatusKlijenta = StatusKlijenta.Prihvaceno;

                            string zadatak = $"NOVI_ZADATAK:{cekajuciKlijent.PocetneKoordinate.X}:{cekajuciKlijent.PocetneKoordinate.Y}:{cekajuciKlijent.KrajnjeKoordinate.X}:{cekajuciKlijent.KrajnjeKoordinate.Y}";
                            byte[] zadatakBytes = Encoding.UTF8.GetBytes(zadatak);
                            taksiVozilo.Stream.Write(zadatakBytes, 0, zadatakBytes.Length);
                            Console.WriteLine("[SERVER] Dodeljen zadatak čekajućem klijentu nakon registracije vozila."); 
                        }
                        */
                    }
                    DodeliZadatke();

                    Console.Write($"[TCP] Novo vozilo se povezalo: {podaciVozila}\n");

                    string potvrda = "Server: Vozilo registrovano";
                    byte[] potvrdaBytes = Encoding.UTF8.GetBytes(potvrda);
                    stream.Write(potvrdaBytes, 0, potvrdaBytes.Length);

                    new Thread(() => ObradaVozila(taksiVozilo)).Start();
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

                    Console.WriteLine($"[UDP] Zahtev od klijenta {klijentEP}: {zahtev}\n");

                    var delovi = zahtev.Split(':');

                    if (delovi.Length != 4)
                    {
                        Console.WriteLine("[GREŠKA] Pogrešan format zahteva");
                        continue;
                    }

                    int x1 = int.Parse(delovi[0]);
                    int y1 = int.Parse(delovi[1]);
                    int x2 = int.Parse(delovi[2]);
                    int y2 = int.Parse(delovi[3]);

                    Klijent klijent = new Klijent
                    {
                        PocetneKoordinate = new Koordinate(x1, y1),
                        KrajnjeKoordinate = new Koordinate(x2, y2),
                        StatusKlijenta = StatusKlijenta.Cekanje
                    };

                    TaksiVozilo slobodnoVozilo = null;

                    slobodnoVozilo = TaksiVozila.Where(v => v.StatusVozila == StatusVozila.Slobodno && v.Stream != null).OrderBy(v => v.KoordinateVozila.Distanca(klijent.PocetneKoordinate)).FirstOrDefault();

                    lock (lockObj)
                    {
                        Klijenti.Add(klijent);

                        //slobodnoVozilo = TaksiVozila.FirstOrDefault(v => v.StatusVozila == StatusVozila.Slobodno && v.Stream != null);

                        // bira najblize vozilo 
                        /*
                        slobodnoVozilo = TaksiVozila.Where(v => v.StatusVozila == StatusVozila.Slobodno && v.Stream != null).OrderBy(v => v.KoordinateVozila.Distanca(klijent.PocetneKoordinate)).FirstOrDefault();

                        if (slobodnoVozilo != null)
                        {
                            slobodnoVozilo.StatusVozila = StatusVozila.Odlazak_Na_Lokaciju;
                            klijent.StatusKlijenta = StatusKlijenta.Prihvaceno;
                        }
                        */
                    }
                    DodeliZadatke();
                    string odgovor = "Server: Vas zahtev je prihvaćen. Čekate dodelu vozila.";
                    byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                    udpClient.Send(odgovorBytes, odgovorBytes.Length, klijentEP);

                    /*if (slobodnoVozilo != null)
                    {
                        string zadatak = $"NOVI_ZADATAK:{x1}:{y1}:{x2}:{y2}";
                        byte[] zadatakBytes = Encoding.UTF8.GetBytes(zadatak);
                        slobodnoVozilo.Stream.Write(zadatakBytes, 0, zadatakBytes.Length);

                        string odgovor = "Server: Vas zahtev je prihvacen. Vozilo je na putu.";
                        byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                        udpClient.Send(odgovorBytes, odgovorBytes.Length, klijentEP);

                        Console.WriteLine("[SERVER] Zadatak poslat vozilu, klijent obavešten.");
                    }
                    else
                    {
                        // AKO NEMA SLOBODNIH VOZILA
                        string odgovor = "Server: Trenutno nema slobodnih vozila.";
                        byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                        udpClient.Send(odgovorBytes, odgovorBytes.Length, klijentEP);

                        Console.WriteLine("[UPOZORENJE] Nema slobodnih vozila.");
                    }*/

                }

                catch (Exception ex)
                {
                    Console.WriteLine($"[GREŠKA UDP] {ex.Message}");
                }
            }
        }
        static void ObradaVozila(TaksiVozilo vozilo)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead = vozilo.Stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string poruka = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    lock (lockObj)
                    {
                        if (poruka == "STIGAO_PO_KLIJENTA")
                        {
                            //vozilo.StatusVozila = StatusVozila.Voznja;
                            var zadatak = Zadaci
        .FirstOrDefault(z => z.Vozilo == vozilo && z.StatusZadatka == StatusZadatka.Aktivan);

                            //if (zadatak != null)
                                //zadatak.StatusZadatka = StatusZadatka.Aktivan;
                        }
                        else if (poruka == "VOZNJA_ZAVRSENA")
                        {
                            var zadatak = Zadaci.FirstOrDefault(z => z.Vozilo == vozilo && z.StatusZadatka == StatusZadatka.Aktivan);

                            if (zadatak != null)
                            {
                                zadatak.StatusZadatka = StatusZadatka.Zavrsen;
                                zadatak.Klijent.StatusKlijenta = StatusKlijenta.Zavrseno;
                            }

                            vozilo.StatusVozila = StatusVozila.Slobodno;
                            vozilo.Zarada += 500;
                            vozilo.PredjenaKilometraza += 5;

                            //var aktivniKlijent = Klijenti.FirstOrDefault(k => k.StatusKlijenta == StatusKlijenta.Prihvaceno);

                            //if (aktivniKlijent != null)
                                //aktivniKlijent.StatusKlijenta = StatusKlijenta.Zavrseno;

                            //if (vozilo.)

                            DodeliZadatke();
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("[TCP] Vozilo se diskonektovalo.");
            }
        }


        static void VizuelizacijaLoop()
        {
            while (true)
            {
                Console.WriteLine("\n-------------------------------------------------\n");
                Thread.Sleep(5000);
                //Console.Clear();
                lock (lockObj)
                {
                    Console.WriteLine("\n\t\t\t=== STATUS VOZILA ===");
                    Console.WriteLine("ID\tPozicijaX\tPozicijaY\tStatus\t\tKm\t\tZarada");
                    int id = 1;
                    foreach (var vozilo in TaksiVozila)
                    {
                        Console.WriteLine($"{id++}\t\t" +
                            $"{vozilo.KoordinateVozila.X}\t" +
                            $"{vozilo.KoordinateVozila.Y}\t\t" +
                            $"{vozilo.StatusVozila}\t\t" +
                            $"{vozilo.PredjenaKilometraza}\t\t" +
                            $"{vozilo.Zarada}");
                    }

                    Console.WriteLine("\n\t\t\t=== AKTIVNI KLIJENTI ===");
                    Console.WriteLine("ID\tPocetnaX\tPocetnaY\tKrajnjaX\tKrajnjaY\tStatus");
                    id = 1;

                    foreach (var k in Klijenti)
                    {
                        Console.WriteLine($"\n{id++}\t\t{k.PocetneKoordinate.X}\t\t" +
                            $"{k.PocetneKoordinate.Y}\t\t" +
                            $"{k.KrajnjeKoordinate.X}\t\t" +
                            $"{k.KrajnjeKoordinate.Y}\t\t" +
                            $"{k.StatusKlijenta}");
                    }
                }
            }
        }

        /*static void DodeliZadatke()
        {
            lock(lockObj)
            {
                var slobodnaVozila = TaksiVozila.Where(v => v.StatusVozila == StatusVozila.Slobodno && v.Stream != null).ToList();

                var cekajuciKlijenti = Klijenti.Where(k => k.StatusKlijenta == StatusKlijenta.Cekanje).ToList();

                foreach (var klijent in cekajuciKlijenti)
                {
                    if (slobodnaVozila.Count == 0)
                        break;

                    var vozilo = slobodnaVozila.OrderBy(v => v.KoordinateVozila.Distanca(klijent.PocetneKoordinate)).First();

                    // dodeli zadatak

                    vozilo.StatusVozila = StatusVozila.Odlazak_Na_Lokaciju;
                    klijent.StatusKlijenta = StatusKlijenta.Prihvaceno;

                    string zadatak = $"NOVI_ZADATAK:{klijent.PocetneKoordinate.X}:{klijent.PocetneKoordinate.Y}:{klijent.KrajnjeKoordinate.X}:{klijent.KrajnjeKoordinate.Y}";

                    byte[] zadatakBytes = Encoding.UTF8.GetBytes(zadatak);
                    vozilo.Stream.Write(zadatakBytes, 0, zadatakBytes.Length);

                    Console.WriteLine($"[SERVER]Zadovoljeno: klijent dodeljen vozilu.");

                    slobodnaVozila.Remove(vozilo);
                }
            }
        }*/

        static void DodeliZadatke()
        {
            lock (lockObj)
            {
                // Sve čekajuće klijente
                var cekajuciKlijenti = Klijenti.Where(k => k.StatusKlijenta == StatusKlijenta.Cekanje).ToList();

                foreach (var klijent in cekajuciKlijenti)
                {
                    // Sve slobodne vozile
                    var slobodnaVozila = TaksiVozila
                        .Where(v => v.StatusVozila == StatusVozila.Slobodno && v.Stream != null)
                        .ToList();

                    if (slobodnaVozila.Count == 0)
                        break;

                    // Izaberi najbliže slobodno vozilo
                    var najblizeVozilo = slobodnaVozila
                        .OrderBy(v => v.KoordinateVozila.Distanca(klijent.PocetneKoordinate))
                        .First();

                    var zadatak = new Zadatak
                    {
                        Klijent = klijent,
                        Vozilo = najblizeVozilo,
                        StatusZadatka = StatusZadatka.Aktivan,
                        PredjenaRazdaljina = 0
                    };

                    Zadaci.Add(zadatak);



                    // Dodeli klijenta
                    najblizeVozilo.StatusVozila = StatusVozila.Odlazak_Na_Lokaciju;
                    klijent.StatusKlijenta = StatusKlijenta.Prihvaceno;

                    string zadatakS = $"NOVI_ZADATAK:{klijent.PocetneKoordinate.X}:{klijent.PocetneKoordinate.Y}:{klijent.KrajnjeKoordinate.X}:{klijent.KrajnjeKoordinate.Y}";
                    byte[] zadatakBytes = Encoding.UTF8.GetBytes(zadatakS);
                    najblizeVozilo.Stream.Write(zadatakBytes, 0, zadatakBytes.Length);

                    //rbTaxi++;
                    Console.WriteLine($"[SERVER] Klijent dodeljen najbližem vozilu.");
                    Console.WriteLine(
                    $"[DISPEČER] Taksi ide po klijenta " +
                    $"({klijent.PocetneKoordinate.X},{klijent.PocetneKoordinate.Y}) → " +
                    $"({klijent.KrajnjeKoordinate.X},{klijent.KrajnjeKoordinate.Y})"
);

                    // Ukloni vozilo iz liste slobodnih da ne bi dodeljeno više puta
                    slobodnaVozila.Remove(najblizeVozilo);
                }
            }
        }

    }
}
