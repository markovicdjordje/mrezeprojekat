using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        public int TrenutnaX { get; set; }
        public int TrenutnaY { get; set; }
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

        public static Klijent Deserialize(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (Klijent)formatter.Deserialize(ms);
            }
        }
    }
}
