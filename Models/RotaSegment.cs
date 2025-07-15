using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WpfAppProlab1.Entities;

namespace WpfAppProlab1.Models
{
    public class RotaSegment
    {
        public string BaslangicDurak { get; set; }
        public string HedefDurak { get; set; }
        public Arac Arac { get; set; }
        public double Ucret { get; set; }
        public int Sure { get; set; }

        public string ToString(Yolcu yolcu, int sira)
        {
            var odeme = new Services.Odeme(this.Ucret);
            double indirimliUcret = odeme.Hesapla(yolcu);

            string mesaj = $"{sira}️⃣ {BaslangicDurak} → {HedefDurak} ({Arac?.Sembol} {Arac?.Ad})";
            mesaj += $"\n⏳ Süre: {Sure} dk";
            mesaj += $"\n💰 Ücret: {Ucret} TL";
            mesaj += $" → {yolcu.GetType().Name.Replace("Yolcu", "")} İndirimi → {indirimliUcret:F2} TL\n";

            return mesaj;
        }
    }
}

