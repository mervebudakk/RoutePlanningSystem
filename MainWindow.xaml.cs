using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Globalization;
using WpfAppProlab1.Entities;
using WpfAppProlab1.Models;
using WpfAppProlab1.Services;


namespace WpfAppProlab1
{
    public enum RotaKriteri
    {
        Sure,
        Ucret
    }

    public partial class MainWindow : Window
    {
        private List<Durak> Duraklar;
        private Taxi TaksiBilgisi;
        private Yolcu aktifYolcu;

        public MainWindow()
        {
            SetBrowserFeatureControl();
            InitializeComponent();
            VerileriYukle();
        }



        private void VerileriYukle()
        {
            try
            {
                string jsonDosyaYolu = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "C:\\MyWorks\\WpfAppProlab1\\WpfAppProlab1\\Data\\veri.json");
                string jsonMetni = File.ReadAllText(jsonDosyaYolu);
                SehirVerisi sehirVerisi = JsonConvert.DeserializeObject<SehirVerisi>(jsonMetni);
                Duraklar = sehirVerisi.Duraklar;
                TaksiBilgisi = sehirVerisi.Taksi;

                durakListBox.Items.Clear();
                foreach (var durak in Duraklar)
                {
                    if (!string.IsNullOrWhiteSpace(durak.Name))
                    {
                        durakListBox.Items.Add(durak.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowResult($"Veri yükleme hatası: {ex.Message}", "HATA");
            }
        }
        private void ShowResult(string message, string title = "SONUÇ")
        {
            string formattedMessage = $"🔹 {title}\n\n{message}";
            ResultsTextBlock.Text = formattedMessage;

            // Sonuçlar bölümüne otomatik scroll
            ResultsScrollViewer.ScrollToTop();
        }
        private void BtnEnYakinDurak_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double kullaniciEnlem = double.Parse(txtMevcutEnlem.Text, CultureInfo.InvariantCulture);
                double kullaniciBoylam = double.Parse(txtMevcutBoylam.Text, CultureInfo.InvariantCulture);

                Durak enYakinDurak = RotaHesaplayici.EnYakinDurak(Duraklar, kullaniciEnlem, kullaniciBoylam);

                if (enYakinDurak != null)
                {
                    ShowResult($"En Yakın Durak: {enYakinDurak.Name} \n" +
                                    $"Mesafe: {RotaHesaplayici.HaversineMesafe(kullaniciEnlem, kullaniciBoylam, enYakinDurak.Enlem, enYakinDurak.Boylam):F2} km");
                }
                else
                {
                    ShowResult("Yakınlarda durak bulunamadı!");
                }
            }
            catch (Exception)
            {
                ShowResult("Lütfen geçerli bir enlem ve boylam girin!");
            }
        }

        private void DurakSecildi(object sender, SelectionChangedEventArgs e)
        {
            if (durakListBox.SelectedItem == null) return;

            string secilenDurakAdi = durakListBox.SelectedItem.ToString();
            Durak secilenDurak = Duraklar.FirstOrDefault(d => d.Name == secilenDurakAdi);

            if (secilenDurak != null)
            {
                if (string.IsNullOrWhiteSpace(txtMevcutEnlem.Text) || string.IsNullOrWhiteSpace(txtMevcutBoylam.Text))
                {
                    txtMevcutEnlem.Text = secilenDurak.Enlem.ToString(CultureInfo.InvariantCulture);
                    txtMevcutBoylam.Text = secilenDurak.Boylam.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    txtHedefEnlem.Text = secilenDurak.Enlem.ToString(CultureInfo.InvariantCulture);
                    txtHedefBoylam.Text = secilenDurak.Boylam.ToString(CultureInfo.InvariantCulture);
                }
            }
        }



        private void BtnRotaHesapla_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double toplamUcret = 0;
                double toplamSure = 0;
                double toplamMesafe = 0;
                string mesaj = "";

                double baslangicEnlem = double.Parse(txtMevcutEnlem.Text, CultureInfo.InvariantCulture);
                double baslangicBoylam = double.Parse(txtMevcutBoylam.Text, CultureInfo.InvariantCulture);
                double hedefEnlem = double.Parse(txtHedefEnlem.Text, CultureInfo.InvariantCulture);
                double hedefBoylam = double.Parse(txtHedefBoylam.Text, CultureInfo.InvariantCulture);

                HaritadaNoktalariGoster(baslangicEnlem, baslangicBoylam, hedefEnlem, hedefBoylam);


                string yolcuTipi = cmbYolcuTipi.Text;
                switch (yolcuTipi)
                {
                    case "Tam":
                        aktifYolcu = new TamYolcu();
                        break;
                    case "Öğrenci":
                        aktifYolcu = new OgrenciYolcu();
                        break;
                    case "65+ Yaş":
                        aktifYolcu = new YasliYolcu();
                        break;
                    default:
                        aktifYolcu = new TamYolcu();
                        break;
                }

                Durak baslangicDurak = RotaHesaplayici.EnYakinDurak(Duraklar, baslangicEnlem, baslangicBoylam);
                Durak hedefDurak = RotaHesaplayici.EnYakinDurak(Duraklar, hedefEnlem, hedefBoylam);

                double baslangicMesafe = RotaHesaplayici.HaversineMesafe(baslangicEnlem, baslangicBoylam, baslangicDurak.Enlem, baslangicDurak.Boylam);
                toplamMesafe += baslangicMesafe;

                double hedefMesafe = RotaHesaplayici.HaversineMesafe(hedefEnlem, hedefBoylam, hedefDurak.Enlem, hedefDurak.Boylam);
                double duraklarArasiMesafe = RotaHesaplayici.HaversineMesafe(baslangicDurak.Enlem, baslangicDurak.Boylam, hedefDurak.Enlem, hedefDurak.Boylam);

                RotaKriteri secilenKriter;

                if (cmbRotaKriteri.SelectedIndex == 0)
                    secilenKriter = RotaKriteri.Sure;
                else
                    secilenKriter = RotaKriteri.Ucret;
                string alternatifRotaSecimi = (cmbAlternatifRota.SelectedItem as ComboBoxItem)?.Content?.ToString();

                if (alternatifRotaSecimi == "Sadece Taksi")
                {
                    string taksiMesaj = RotaHesaplayici.SadeceTaksiRotasi(
                        baslangicEnlem, baslangicBoylam,
                        hedefEnlem, hedefBoylam,
                        TaksiBilgisi
                    );

                    // Harita çizimi için rota noktaları listesi
                    var noktalar = new List<(double Lat, double Lng, string Label)>
{
    (baslangicEnlem, baslangicBoylam, "Başlangıç"),
    (hedefEnlem, hedefBoylam, "Hedef")
};
                    HaritadaRotaCiz(noktalar);


                    ShowResult(taksiMesaj, "🚖 Taksi Rota Bilgisi");

                    return;
                }

                if (alternatifRotaSecimi == "Sadece Otobüs")
                {
                    var rota = RotaHesaplayici.SadeceOtobusRotasi(Duraklar, baslangicDurak, hedefDurak);

                    if (rota == null || !rota.Any())
                    {
                        ShowResult("Otobüs ile ulaşılabilir bir rota bulunamadı.", "Otobüs Rotası");

                        return;
                    }

                    mesaj = "🚌 Sadece Otobüs Rotası:\n\n";
                    toplamSure = 0;
                    toplamUcret = 0;

                    var noktalar = new List<(double Lat, double Lng, string Label)>();


                    for (int i = 0; i < rota.Count; i++)
                    {
                        var segment = rota[i];
                        mesaj += segment.ToString(aktifYolcu, i + 1) + "\n";

                        var basDurak = Duraklar.FirstOrDefault(d => d.Name == segment.BaslangicDurak);
                        var hedefDurakSegment = Duraklar.FirstOrDefault(d => d.Name == segment.HedefDurak);
                        if (basDurak != null && hedefDurakSegment != null)
                        {
                            toplamMesafe += RotaHesaplayici.HaversineMesafe(basDurak.Enlem, basDurak.Boylam, hedefDurakSegment.Enlem, hedefDurakSegment.Boylam);

                            if (!noktalar.Contains((basDurak.Enlem, basDurak.Boylam, basDurak.Name)))
                                noktalar.Add((basDurak.Enlem, basDurak.Boylam, basDurak.Name));

                            if (!noktalar.Contains((hedefDurakSegment.Enlem, hedefDurakSegment.Boylam, hedefDurakSegment.Name)))
                                noktalar.Add((hedefDurakSegment.Enlem, hedefDurakSegment.Boylam, hedefDurak.Name));
                        }

                        toplamSure += segment.Sure;
                        toplamUcret += new Odeme(segment.Ucret).Hesapla(aktifYolcu);
                    }

                    mesaj += $"\n📊 **Toplam Süre:** {toplamSure} dk";
                    mesaj += $"\n💰 **Toplam Ücret:** {toplamUcret:F2} TL";
                    mesaj += $"\n📏 **Toplam Mesafe:** {toplamMesafe:F2} km";

                    HaritadaRotaCiz(noktalar);

                    ShowResult(mesaj, "Sadece Otobüs Rotası");
                    return;

                }

                if (alternatifRotaSecimi == "Tramvay Öncelikli")
                {
                    var rota = RotaHesaplayici.TramvayOncelikliRota(Duraklar, baslangicDurak, hedefDurak);

                    if (rota == null || !rota.Any())
                    {
                        ShowResult("Tramvay içeren bir rota bulunamadı.", "Tramvay Öncelikli Rota");
                        return;
                    }

                    mesaj = "🚋 Tramvay Öncelikli Rota:\n\n";
                    toplamSure = 0;
                    toplamUcret = 0;
                    toplamMesafe = 0;

                    var noktalar = new List<(double Lat, double Lng, string Label)>();


                    for (int i = 0; i < rota.Count; i++)
                    {
                        var segment = rota[i];
                        mesaj += segment.ToString(aktifYolcu, i + 1) + "\n";

                        var basDurak = Duraklar.FirstOrDefault(d => d.Name == segment.BaslangicDurak);
                        var hedefDurakSegment = Duraklar.FirstOrDefault(d => d.Name == segment.HedefDurak);

                        if (basDurak != null && hedefDurakSegment != null)
                        {
                            toplamMesafe += RotaHesaplayici.HaversineMesafe(
                                basDurak.Enlem, basDurak.Boylam,
                                hedefDurakSegment.Enlem, hedefDurakSegment.Boylam
                            );

                            if (!noktalar.Contains((basDurak.Enlem, basDurak.Boylam, basDurak.Name)))
                                noktalar.Add((basDurak.Enlem, basDurak.Boylam, basDurak.Name));
                            if (!noktalar.Contains((hedefDurakSegment.Enlem, hedefDurakSegment.Boylam, hedefDurakSegment.Name)))
                                noktalar.Add((hedefDurakSegment.Enlem, hedefDurakSegment.Boylam, hedefDurakSegment.Name));
                        }

                        toplamSure += segment.Sure;
                        toplamUcret += new Odeme(segment.Ucret).Hesapla(aktifYolcu);
                    }

                    mesaj += $"\n📊 **Toplam Süre:** {toplamSure} dk";
                    mesaj += $"\n💰 **Toplam Ücret:** {toplamUcret:F2} TL";
                    mesaj += $"\n📏 **Toplam Mesafe:** {toplamMesafe:F2} km";

                    HaritadaRotaCiz(noktalar); // <-- rota çizdir

                    ShowResult(mesaj, "Tramvay Öncelikli Rota");
                    return;
                }


                if (alternatifRotaSecimi == "En Az Aktarmalı")
                {
                    var rota = RotaHesaplayici.DijkstraEnKisaRota(Duraklar, baslangicDurak, hedefDurak, secilenKriter);

                    if (rota == null || !rota.Any())
                    {
                        MessageBox.Show("En az aktarmalı rota bulunamadı.", "Rota Bilgisi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int aktarmaSayisi = rota.Count(seg => seg.Arac is Transfer);
                    string mesajAktarma = $"🔁 En Az Aktarmalı Rota Bulundu (Toplam Aktarma: {aktarmaSayisi})\n\n";
                    double toplamSureEA = 0;
                    double toplamUcretEA = 0;
                    double toplamMesafeEA = 0;

                    var noktalar = new List<(double Lat, double Lng, string Label)>();


                    for (int i = 0; i < rota.Count; i++)
                    {
                        var segment = rota[i];
                        mesajAktarma += segment.ToString(aktifYolcu, i + 1) + "\n";

                        var basDurak = Duraklar.FirstOrDefault(d => d.Name == segment.BaslangicDurak);
                        var hedefDurakSegment = Duraklar.FirstOrDefault(d => d.Name == segment.HedefDurak);

                        if (basDurak != null && hedefDurakSegment != null)
                        {
                            toplamMesafeEA += RotaHesaplayici.HaversineMesafe(basDurak.Enlem, basDurak.Boylam, hedefDurakSegment.Enlem, hedefDurakSegment.Boylam);

                            if (noktalar.Count == 0 || noktalar.Last() != (basDurak.Enlem, basDurak.Boylam, basDurak.Name))
                                noktalar.Add((basDurak.Enlem, basDurak.Boylam, basDurak.Name)); // çift eklenmesin

                            noktalar.Add((hedefDurakSegment.Enlem, hedefDurakSegment.Boylam, hedefDurakSegment.Name));
                        }

                        toplamSureEA += segment.Sure;
                        toplamUcretEA += new Odeme(segment.Ucret).Hesapla(aktifYolcu);
                    }

                    mesajAktarma += $"\n📊 **Toplam Süre:** {toplamSureEA} dk";
                    mesajAktarma += $"\n💰 **Toplam Ücret:** {toplamUcretEA:F2} TL";
                    mesajAktarma += $"\n📏 **Toplam Mesafe:** {toplamMesafeEA:F2} km";

                    HaritadaRotaCiz(noktalar); // 🧭 ROTA ÇİZİMİ

                    ShowResult(mesajAktarma, "En Az Aktarmalı Rota");
                    return;
                }




                var enKisaRota = RotaHesaplayici.DijkstraEnKisaRota(Duraklar, baslangicDurak, hedefDurak, secilenKriter);

                if (enKisaRota == null || !enKisaRota.Any())
                {
                    MessageBox.Show("Ulaşım için uygun bir rota bulunamadı!", "Rota Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string kriterMetni = secilenKriter == RotaKriteri.Sure ? "🕒 Süreye Göre" : "💰 Ücrete Göre";
                mesaj = $"🔍 Seçilen Kriter: {kriterMetni}\n📍 Kullanıcı Konumuna En Yakın Durak: \n 🔹 {baslangicDurak.Name} ({baslangicMesafe * 1000:F2}m) → ";

                if (baslangicMesafe <= 3)
                    mesaj += "🚶 Yürüme = 0 TL";
                else
                    mesaj += $"🆓 Taksi = {baslangicMesafe:F2} km = {TaksiUcretiHesapla(duraklarArasiMesafe):F2} TL";
                mesaj += "\n\n📍ROTA DETAYLARI📍\n";

                var oncekiSegment = enKisaRota[0];

                for (int i = 0; i < enKisaRota.Count; i++)
                {
                    var segment = enKisaRota[i];

                    var basDurak = Duraklar.FirstOrDefault(d => d.Name == segment.BaslangicDurak);
                    var hedefDurakSegment = Duraklar.FirstOrDefault(d => d.Name == segment.HedefDurak);

                    if (basDurak != null && hedefDurakSegment != null)
                    {
                        double mesafe = RotaHesaplayici.HaversineMesafe(basDurak.Enlem, basDurak.Boylam, hedefDurakSegment.Enlem, hedefDurakSegment.Boylam);
                        toplamMesafe += mesafe;
                        if (segment.Arac is Transfer)
                        {
                            mesaj += $"🔄 (Transfer Mesafesi: {mesafe:F2} km)\n";
                        }
                    }

                    string aracTipi = $"{segment.Arac?.Sembol} {segment.Arac?.Ad}";

                    Odeme odeme = new Odeme(segment.Ucret);
                    double ucret = odeme.Hesapla(aktifYolcu);
                    toplamUcret += ucret;
                    toplamSure += segment.Sure;

                    mesaj += segment.ToString(aktifYolcu, i + 1);

                    mesaj += "\n";
                }

                double hedefNoktaMesafe = RotaHesaplayici.HaversineMesafe(hedefDurak.Enlem, hedefDurak.Boylam, hedefEnlem, hedefBoylam);
                toplamMesafe += hedefNoktaMesafe;

                if (hedefNoktaMesafe <= 3)
                {
                    mesaj += $"\n🚶 Hedefe Yürüyerek Ulaşım ({hedefNoktaMesafe:F2} km) = 0 TL";
                    double yurumeSuresi = RotaHesaplayici.YurumeyleGecenSureDakika(hedefNoktaMesafe);
                    toplamSure += yurumeSuresi;
                }
                else
                {
                    double taksiUcret = TaksiUcretiHesapla(hedefNoktaMesafe);
                    mesaj += $"\n🚖 Hedefe Taksi ile Ulaşım ({hedefNoktaMesafe:F2} km) = {taksiUcret:F2} TL\n";
                    double taksiSuresi = RotaHesaplayici.TaksiylaGecenSureDakika(hedefNoktaMesafe, TaksiBilgisi.averageSpeedKmh);
                    toplamSure += taksiSuresi;
                    toplamUcret += taksiUcret;
                }

                mesaj += $"\n📊 **Toplam Bilgiler:**";
                mesaj += $"\n⏳ **Toplam Süre:** {toplamSure} dk";
                mesaj += $"\n💰 **Toplam Ücret:** {toplamUcret:F2} TL";
                mesaj += $"\n📏 **Toplam Mesafe:** {toplamMesafe:F2} km";

                ShowResult(mesaj, "Rota ve Maliyet Hesaplama");
            }
            catch (Exception)
            {
                ShowResult("", "Hata");
            }

        }

        private void HaritadaRotaCiz(List<(double Lat, double Lng, string Label)> noktalar)
        {
            try
            {
                var rotaObjeleri = noktalar.Select(n => new { lat = n.Lat, lng = n.Lng, label = n.Label }).ToList();
                string json = JsonConvert.SerializeObject(rotaObjeleri);
                HaritaGoruntuleyici.InvokeScript("drawRoute", new string[] { json });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rota çizim hatası: " + ex.Message);
            }
        }





        private void HaritadaNoktalariGoster(double baslangicLat, double baslangicLng, double hedefLat, double hedefLng)
        {
            try
            {
                // Eski işaretçileri temizle
                HaritaGoruntuleyici.InvokeScript("eval", new string[] { "clearMarkers();" });

                // Yeni işaretçileri ekle
                HaritaGoruntuleyici.InvokeScript("eval", new string[] {
            $"addMarker({baslangicLat.ToString(CultureInfo.InvariantCulture)}, {baslangicLng.ToString(CultureInfo.InvariantCulture)}, 'Başlangıç Noktası');"
        });

                HaritaGoruntuleyici.InvokeScript("eval", new string[] {
            $"addMarker({hedefLat.ToString(CultureInfo.InvariantCulture)}, {hedefLng.ToString(CultureInfo.InvariantCulture)}, 'Hedef Noktası');"
        });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Haritada işaretleme yapılamadı: " + ex.Message);
            }
        }


        private double TaksiUcretiHesapla(double mesafe)
        {
            return TaksiBilgisi.openingFee + (mesafe * TaksiBilgisi.costPerKm);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Map.html");

            if (File.Exists(path))
            {
                HaritaGoruntuleyici.Navigate(new Uri(path));
            }
            else
            {
                MessageBox.Show("Map.html dosyası bulunamadı:\n" + path);
            }
        }



        private void SetBrowserFeatureControl()
        {
            try
            {
                var appName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                    @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                {
                    key?.SetValue(appName, 11001, Microsoft.Win32.RegistryValueKind.DWord); // IE11 emülasyonu
                }
            }
            catch (Exception ex)
            {
                // Gerekirse logla
                MessageBox.Show("WebBrowser ayarı yapılamadı: " + ex.Message);
            }
        }



    }


}
