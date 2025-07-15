using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppProlab1.Entities
{
    public abstract class Yolcu
    {
        public abstract double IndirimliUcret(double ucret);
    }

    public class TamYolcu : Yolcu
    {
        public override double IndirimliUcret(double ucret) => ucret;
    }

    public class OgrenciYolcu : Yolcu
    {
        public override double IndirimliUcret(double ucret) => ucret * 0.5;
    }

    public class YasliYolcu : Yolcu
    {
        public override double IndirimliUcret(double ucret) => 0;
    }
}

