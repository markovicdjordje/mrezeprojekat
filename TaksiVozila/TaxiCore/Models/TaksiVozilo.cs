using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TaxiCore.Enums;
using TaxiCore.Models;

namespace TaxiCore.Models
{
    [Serializable]
    public class TaksiVozilo
    {
        [NonSerialized]
        public TcpClient TcpClient;
        [NonSerialized]
        public NetworkStream Stream;

        public Koordinate KoordinateVozila {  get; set; }
        public StatusVozila StatusVozila { get; set; }
        public double PredjenaKilometraza { get; set; }
        public double Zarada { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                return ms.ToArray();
            }
        }

        public static TaksiVozilo Deserialize(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (TaksiVozilo)formatter.Deserialize(ms);
            }
        }

        public static string Header()
        {
            return $@"| {"KoordinateX",-5} | {"KoordinateY",-5} | {"Status",-10} | {"Kilometraza",-5} | {"Zarada",-5}" +
                   "\n--------------------------------------------------------------------------------------------------------------------";
        }

        public override string ToString()
        {
            return $@"| {KoordinateVozila.X,-11} | {KoordinateVozila.Y,-11} | {StatusVozila,-10} | {PredjenaKilometraza,-11}km | {Zarada,-5}din";
        }


    }
}
