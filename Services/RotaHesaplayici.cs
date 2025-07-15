using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WpfAppProlab1.Entities;
using WpfAppProlab1.Models;


namespace WpfAppProlab1.Services
{
    public class RotaHesaplayici
    {
        public static double HaversineMesafe(double enlem1, double boylam1, double enlem2, double boylam2)
        {
            double R = 6371;
            double dLat = DereceToRadyan(enlem2 - enlem1);
            double dLon = DereceToRadyan(boylam2 - boylam1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DereceToRadyan(enlem1)) * Math.Cos(DereceToRadyan(enlem2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        private static double DereceToRadyan(double derece)
        {
            return derece * (Math.PI / 180);
        }
        public static Durak EnYakinDurak(List<Durak> duraklar, double kullaniciEnlem, double kullaniciBoylam)
        {
            return duraklar
                .OrderBy(durak => HaversineMesafe(kullaniciEnlem, kullaniciBoylam, durak.Enlem, durak.Boylam))
                .FirstOrDefault();
        }
        public static bool DuraklarBagliMi(Durak baslangic, Durak hedef)
        {
            if (baslangic == null || hedef == null)
                return false;

            return baslangic.SonrakiDuraklar != null && baslangic.SonrakiDuraklar.Any(d => d.stopId == hedef.Id);
        }
        public static Durak EnYakinBaglantiliDurak(Durak baslangic, Durak hedef, List<Durak> duraklar)
        {
            foreach (var durak in duraklar)
            {
                if (DuraklarBagliMi(baslangic, durak) && DuraklarBagliMi(durak, hedef))
                {
                    return durak;
                }
            }
            return null;
        }

        public static List<RotaSegment> TaksiRotasiOlustur(double baslangicEnlem, double baslangicBoylam, double hedefEnlem, double hedefBoylam, Taxi taksi)
        {
            double mesafe = RotaHesaplayici.HaversineMesafe(baslangicEnlem, baslangicBoylam, hedefEnlem, hedefBoylam);
            int sure = (int)Math.Ceiling(RotaHesaplayici.TaksiylaGecenSureDakika(mesafe, taksi.averageSpeedKmh));
            double ucret = taksi.openingFee + mesafe * taksi.costPerKm;

            return new List<RotaSegment>
            {
                new RotaSegment
                {
                    BaslangicDurak = "Mevcut Konum",
                    HedefDurak = "Hedef Nokta",
                    Arac = new Taksi(),
                    Ucret = ucret,
                    Sure = sure
                }
            };
        }

        public static List<RotaSegment> DijkstraEnKisaRota(List<Durak> duraklar, Durak baslangic, Durak hedef, RotaKriteri kriter)
        {
            var mesafeler = new Dictionary<string, double>();
            var oncekiDuraklar = new Dictionary<string, string>();
            var ziyaretEdilenler = new HashSet<string>();
            var oncelikKuyrugu = new PriorityQueue<string, double>();

            foreach (var durak in duraklar)
            {
                mesafeler[durak.Id] = double.MaxValue;
            }

            mesafeler[baslangic.Id] = 0;
            oncelikKuyrugu.Enqueue(baslangic.Id, 0);

            while (oncelikKuyrugu.Count > 0)
            {
                var mevcutDurakId = oncelikKuyrugu.Dequeue();
                if (mevcutDurakId == hedef.Id) break;

                var mevcutDurak = duraklar.FirstOrDefault(d => d.Id == mevcutDurakId);
                if (mevcutDurak == null || ziyaretEdilenler.Contains(mevcutDurakId)) continue;

                ziyaretEdilenler.Add(mevcutDurakId);

                if (mevcutDurak.SonrakiDuraklar != null)
                {
                    foreach (var sonrakiBaglanti in mevcutDurak.SonrakiDuraklar)
                    {
                        var sonrakiDurak = duraklar.FirstOrDefault(d => d.Id == sonrakiBaglanti.stopId);
                        if (sonrakiDurak == null) continue;

                        double ekMaliyet = 0;

                        switch (kriter)
                        {
                            case RotaKriteri.Sure:
                                ekMaliyet = sonrakiBaglanti.Sure;
                                break;
                            case RotaKriteri.Ucret:
                                ekMaliyet = sonrakiBaglanti.Ucret;
                                break;
                        }

                        double yeniToplamMaliyet = mesafeler[mevcutDurakId] + ekMaliyet;

                        if (yeniToplamMaliyet < mesafeler[sonrakiBaglanti.stopId])
                        {
                            mesafeler[sonrakiBaglanti.stopId] = yeniToplamMaliyet;
                            oncekiDuraklar[sonrakiBaglanti.stopId] = mevcutDurakId;
                            oncelikKuyrugu.Enqueue(sonrakiBaglanti.stopId, yeniToplamMaliyet);
                        }
                    }
                }

                if (mevcutDurak.Aktar != null)
                {
                    string transferDurakId = mevcutDurak.Aktar.TransferStopId;
                    var transferDurak = duraklar.FirstOrDefault(d => d.Id == transferDurakId);
                    if (transferDurak != null && !ziyaretEdilenler.Contains(transferDurakId))
                    {
                        double transferMaliyet = 0;

                        switch (kriter)
                        {
                            case RotaKriteri.Sure:
                                transferMaliyet = mevcutDurak.Aktar.TransferSure;
                                break;
                            case RotaKriteri.Ucret:
                                transferMaliyet = mevcutDurak.Aktar.TransferUcret;
                                break;
                        }

                        double yeniToplamMaliyet = mesafeler[mevcutDurakId] + transferMaliyet;

                        if (yeniToplamMaliyet < mesafeler[transferDurakId])
                        {
                            mesafeler[transferDurakId] = yeniToplamMaliyet;
                            oncekiDuraklar[transferDurakId] = mevcutDurakId;
                            oncelikKuyrugu.Enqueue(transferDurakId, yeniToplamMaliyet);
                        }
                    }
                }

            }

            if (!oncekiDuraklar.ContainsKey(hedef.Id))
                return null;

            var rota = new List<RotaSegment>();
            string suankiDurakId = hedef.Id;

            while (oncekiDuraklar.ContainsKey(suankiDurakId))
            {
                var oncekiDurakId = oncekiDuraklar[suankiDurakId];
                var oncekiDurak = duraklar.First(d => d.Id == oncekiDurakId);
                var mevcutDurak = duraklar.First(d => d.Id == suankiDurakId);

                var baglanti = oncekiDurak.SonrakiDuraklar?.FirstOrDefault(s => s.stopId == mevcutDurak.Id);

                string segmentType = oncekiDurak.Type;

                if (oncekiDurak.Type.Trim().ToLower() != mevcutDurak.Type.Trim().ToLower())
                {
                    segmentType = "transfer";
                }

                int sure = 0;
                double ucret = 0;

                if (segmentType == "transfer")
                {
                    sure = oncekiDurak.Aktar?.TransferSure ?? 0;
                    ucret = oncekiDurak.Aktar?.TransferUcret ?? 0;
                }
                else
                {
                    sure = baglanti?.Sure ?? 0;
                    ucret = baglanti?.Ucret ?? 0;
                }

                Arac arac;
                switch (segmentType.ToLower())
                {
                    case "bus":
                        arac = new Otobus();
                        break;
                    case "tram":
                        arac = new Tramvay();
                        break;
                    case "transfer":
                        arac = new Transfer();
                        break;
                    default:
                        arac = null;
                        break;
                }

                rota.Insert(0, new RotaSegment
                {
                    BaslangicDurak = oncekiDurak.Name,
                    HedefDurak = mevcutDurak.Name,
                    Arac = arac,
                    Ucret = ucret,
                    Sure = sure
                });
                suankiDurakId = oncekiDurakId;
            }
            return rota;
        }

        public static List<RotaSegment> EnAzAktarmaRotasi(List<Durak> duraklar, Durak baslangic, Durak hedef)
        {
            var aktarimSayilari = new Dictionary<string, int>();
            var oncekiDuraklar = new Dictionary<string, string>();
            var oncekiAracTipleri = new Dictionary<string, string>();
            var ziyaretEdilenler = new HashSet<string>();
            var oncelikKuyrugu = new PriorityQueue<string, int>();

            foreach (var durak in duraklar)
                aktarimSayilari[durak.Id] = int.MaxValue;

            aktarimSayilari[baslangic.Id] = 0;
            oncekiAracTipleri[baslangic.Id] = baslangic.Type;
            oncelikKuyrugu.Enqueue(baslangic.Id, 0);

            while (oncelikKuyrugu.Count > 0)
            {
                var mevcutId = oncelikKuyrugu.Dequeue();
                if (mevcutId == hedef.Id) break;

                var mevcut = duraklar.FirstOrDefault(d => d.Id == mevcutId);
                if (mevcut == null || ziyaretEdilenler.Contains(mevcutId)) continue;
                ziyaretEdilenler.Add(mevcutId);

                if (mevcut.SonrakiDuraklar != null)
                {
                    foreach (var baglanti in mevcut.SonrakiDuraklar)
                    {
                        var sonraki = duraklar.FirstOrDefault(d => d.Id == baglanti.stopId);
                        if (sonraki == null) continue;

                        string oncekiTip = oncekiAracTipleri[mevcutId];
                        string yeniTip = sonraki.Type;

                        int ekAktarma = (oncekiTip != yeniTip) ? 1 : 0;
                        int yeniToplamAktarma = aktarimSayilari[mevcutId] + ekAktarma;

                        if (yeniToplamAktarma < aktarimSayilari[sonraki.Id])
                        {
                            aktarimSayilari[sonraki.Id] = yeniToplamAktarma;
                            oncekiDuraklar[sonraki.Id] = mevcutId;
                            oncekiAracTipleri[sonraki.Id] = yeniTip;
                            oncelikKuyrugu.Enqueue(sonraki.Id, yeniToplamAktarma);
                        }
                    }
                }

                if (mevcut.Aktar != null)
                {
                    var transferDurak = duraklar.FirstOrDefault(d => d.Id == mevcut.Aktar.TransferStopId);
                    if (transferDurak == null || ziyaretEdilenler.Contains(transferDurak.Id)) continue;

                    string oncekiTip = oncekiAracTipleri[mevcutId];
                    string yeniTip = transferDurak.Type;

                    int ekAktarma = (oncekiTip != yeniTip) ? 1 : 0;
                    int yeniToplamAktarma = aktarimSayilari[mevcutId] + ekAktarma;

                    if (yeniToplamAktarma < aktarimSayilari[transferDurak.Id])
                    {
                        aktarimSayilari[transferDurak.Id] = yeniToplamAktarma;
                        oncekiDuraklar[transferDurak.Id] = mevcutId;
                        oncekiAracTipleri[transferDurak.Id] = yeniTip;
                        oncelikKuyrugu.Enqueue(transferDurak.Id, yeniToplamAktarma);
                    }
                }
            }

            if (!oncekiDuraklar.ContainsKey(hedef.Id))
                return null;

            return RotaOlustur(duraklar, oncekiDuraklar, baslangic.Id, hedef.Id);
        }

        public static List<RotaSegment> RotaOlustur(List<Durak> duraklar, Dictionary<string, string> oncekiDuraklar, string baslangicId, string hedefId)
        {
            var rota = new List<RotaSegment>();
            string suankiId = hedefId;

            while (oncekiDuraklar.ContainsKey(suankiId))
            {
                string oncekiId = oncekiDuraklar[suankiId];
                var baslangicDurak = duraklar.First(d => d.Id == oncekiId);
                var hedefDurak = duraklar.First(d => d.Id == suankiId);

                string segmentType = baslangicDurak.Type;
                if (baslangicDurak.Type.Trim().ToLower() != hedefDurak.Type.Trim().ToLower())
                    segmentType = "transfer";

                int sure = 0;
                double ucret = 0;

                if (segmentType == "transfer")
                {
                    sure = baslangicDurak.Aktar?.TransferSure ?? 0;
                    ucret = baslangicDurak.Aktar?.TransferUcret ?? 0;
                }
                else
                {
                    var baglanti = baslangicDurak.SonrakiDuraklar.FirstOrDefault(s => s.stopId == hedefDurak.Id);
                    sure = baglanti?.Sure ?? 0;
                    ucret = baglanti?.Ucret ?? 0;
                }

                Arac arac;
                switch (segmentType.ToLower())
                {
                    case "bus":
                        arac = new Otobus();
                        break;
                    case "tram":
                        arac = new Tramvay();
                        break;
                    case "transfer":
                        arac = new Transfer();
                        break;
                    default:
                        arac = null;
                        break;
                }

                rota.Insert(0, new RotaSegment
                {
                    BaslangicDurak = baslangicDurak.Name,
                    HedefDurak = hedefDurak.Name,
                    Arac = arac,
                    Ucret = ucret,
                    Sure = sure
                });

                suankiId = oncekiId;
            }

            return rota;
        }


        public static double YurumeyleGecenSureDakika(double mesafeKm)
        {
            double yurumeHiziKmh = 5.0;
            return (mesafeKm / yurumeHiziKmh) * 60.0;
        }

        public static double TaksiylaGecenSureDakika(double mesafeKm, double taksiHiziKmh)
        {
            return (mesafeKm / taksiHiziKmh) * 60.0;
        }

        public static string SadeceTaksiRotasi(double baslangicEnlem, double baslangicBoylam, double hedefEnlem, double hedefBoylam, Taxi taksi)
        {
            double mesafeKm = HaversineMesafe(baslangicEnlem, baslangicBoylam, hedefEnlem, hedefBoylam);
            double ucret = taksi.openingFee + (mesafeKm * taksi.costPerKm);
            double sure = TaksiylaGecenSureDakika(mesafeKm, taksi.averageSpeedKmh);

            string mesaj = $"🚖 Sadece Taksi Rota Bilgisi:\n";
            mesaj += $"📍 Başlangıç: ({baslangicEnlem:F4}, {baslangicBoylam:F4})\n";
            mesaj += $"🎯 Hedef: ({hedefEnlem:F4}, {hedefBoylam:F4})\n";
            mesaj += $"📏 Mesafe: {mesafeKm:F2} km\n";
            mesaj += $"⏳ Süre: {sure:F1} dakika\n";
            mesaj += $"💰 Ücret: {ucret:F2} TL";

            return mesaj;
        }

        public static List<RotaSegment> SadeceOtobusRotasi(List<Durak> duraklar, Durak baslangic, Durak hedef)
        {
            return DijkstraEnKisaRota(duraklar, baslangic, hedef, RotaKriteri.Sure)?.Where(seg => seg.Arac is Otobus).ToList();
        }

        public static List<RotaSegment> TramvayOncelikliRota(List<Durak> duraklar, Durak baslangic, Durak hedef)
        {
            var tumRota = DijkstraEnKisaRota(duraklar, baslangic, hedef, RotaKriteri.Sure);

            if (tumRota == null || !tumRota.Any())
                return null;

            if (tumRota.Any(s => s.Arac is Tramvay))
                return tumRota;

            return null;
        }
    }

}
