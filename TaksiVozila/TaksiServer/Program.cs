using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaksiServer
{
    class Program
    {
        private static TcpListener tcpListener;
        private static UdpClient udpClient;
        private static TcpClient voziloKlijent;
        private static NetworkStream voziloStream;

        private const int TCP_PORT = 5000; // Port za vozila
        private const int UDP_PORT = 5001; // Port za klijente

        private static string statusVozila = null;

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

            // Glavna petlja za UDP zahteve od klijenata
            PrihvatiKlijente();

        }

        // TCP - Prihvati povezivanje vozila
        static void PrihvatiVozilo()
        {
            try
            {
                voziloKlijent = tcpListener.AcceptTcpClient();
                voziloStream = voziloKlijent.GetStream();
                Console.WriteLine("[TCP] ✓ Vozilo se povezalo!\n");

                // Primi podatke od vozila
                byte[] buffer = new byte[1024];
                int bytesRead = voziloStream.Read(buffer, 0, buffer.Length);
                string podaciVozila = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Console.WriteLine($"[TCP] Primljeni podaci od vozila:");
                Console.WriteLine($"      {podaciVozila}\n");

                if (podaciVozila.Contains("slobodan"))
                    statusVozila = "slobodan";
                else
                    statusVozila = "zauzet";
                
                // Pošalji potvrdu vozilu
                string potvrda = "Server: Vozilo registrovano";
                byte[] potvrdaBytes = Encoding.UTF8.GetBytes(potvrda);
                voziloStream.Write(potvrdaBytes, 0, potvrdaBytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GREŠKA TCP] {ex.Message}");
            }
        }

        // UDP - Prihvati zahteve od klijenata
        static void PrihvatiKlijente()
        {
            try
            {
                while (true)
                {
                    IPEndPoint klijentEndPoint = new IPEndPoint(IPAddress.Any, 0);

                    // Primi zahtev od klijenta
                    byte[] primljeniPodaci = udpClient.Receive(ref klijentEndPoint);
                    string zahtevKlijenta = Encoding.UTF8.GetString(primljeniPodaci);

                    Console.WriteLine($"[UDP] ✓ Primljen zahtev od klijenta {klijentEndPoint}:");
                    Console.WriteLine($"      {zahtevKlijenta}\n");


                    if (voziloStream == null)
                    {
                        string odgovor = "Server: Vozilo još nije povezano, pokušajte kasnije.";
                        byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                        udpClient.Send(odgovorBytes, odgovorBytes.Length, klijentEndPoint);
                        Console.WriteLine("[UPOZORENJE] Vozilo još nije povezano.\n");
                        continue;
                    }

                    // Obradi zahtev i pošalji vozilu
                    if (statusVozila == "slobodan")
                    {
                        Console.WriteLine("[SERVER] Prosleđujem zadatak vozilu...");
                        string zadatak = $"NOVI ZADATAK: {zahtevKlijenta}";
                        byte[] zadatakBytes = Encoding.UTF8.GetBytes(zadatak);
                        voziloStream.Write(zadatakBytes, 0, zadatakBytes.Length);
                        Console.WriteLine("[TCP] ✓ Zadatak poslat vozilu!\n");

                        // Pošalji potvrdu klijentu
                        string odgovor = "Server: Vaš zahtev je prihvaćen. Vozilo je u putu!";
                        byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                        udpClient.Send(odgovorBytes, odgovorBytes.Length, klijentEndPoint);
                        Console.WriteLine("[UDP] ✓ Potvrda poslata klijentu!\n");
                        Console.WriteLine("==========================================\n");
                        statusVozila = "zauzet";
                    }
                    else
                    {
                        string odgovor = "Server: Trenutno nema slobodnih vozila";
                        byte[] odgovorBytes = Encoding.UTF8.GetBytes(odgovor);
                        udpClient.Send(odgovorBytes, odgovorBytes.Length, klijentEndPoint);
                        Console.WriteLine("[UPOZORENJE] Nema povezanih vozila!\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GREŠKA UDP] {ex.Message}");
            }
        }
    }
}
