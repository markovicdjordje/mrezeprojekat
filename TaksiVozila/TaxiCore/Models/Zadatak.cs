using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using TaxiCore.Enums;

namespace TaxiCore.Models
{
    [Serializable]
    public class Zadatak
    {
        public Klijent Klijent { get; set; }
        public TaksiVozilo Vozilo { get; set; }
        public StatusZadatka StatusZadatka { get; set; }
        public double PredjenaRazdaljina { get; set; }

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
