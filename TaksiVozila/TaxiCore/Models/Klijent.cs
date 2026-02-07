using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TaxiCore.Enums;

namespace TaxiCore.Models
{
    [Serializable]
    public class Klijent
    {
        public int PocetnaX { get; set; }
        public int PocetnaY { get; set; }
        public int KrajnjaX { get; set; }
        public int KrajnjaY { get; set; }

        public StatusKlijenta StatusKlijenta { get; set; }

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
