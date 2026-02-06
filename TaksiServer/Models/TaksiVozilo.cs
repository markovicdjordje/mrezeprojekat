using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TaksiServer.Enumeracije;

namespace TaksiServer.Models
{
    public class TaksiVozilo
    {
        public int TrenutnaX {  get; set; }
        public int TrenutnaY { get; set; }
        public StatusVozila StatusVozila { get; set; }
        public int PredjenaKilometraza { get; set; }
        public int zarada {  get; set; }
    }
}
