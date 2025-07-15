using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfAppProlab1.Entities;

namespace WpfAppProlab1.Services
{
    public class Odeme
    {
        private readonly double _ucret;

        public Odeme(double ucret)
        {
            _ucret = ucret;
        }

        public double Hesapla(Yolcu yolcu)
        {
            return yolcu.IndirimliUcret(_ucret);
        }
    }
}

