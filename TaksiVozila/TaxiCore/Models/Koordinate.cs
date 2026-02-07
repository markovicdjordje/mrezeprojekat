using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiCore.Models
{
    [Serializable]
    public class Koordinate
    {
        public int X {  get; set; }
        public int Y { get; set; }

        public Koordinate() { }

        public Koordinate (int x, int y)
        {
            X = x;
            Y = y;
        }

        public double Distanca (Koordinate drugi)
        {
            int dx = X - drugi.X;
            int dy = Y - drugi.Y;
            return Math.Sqrt(dx * dx  + dy * dy);
        }
    }
}
