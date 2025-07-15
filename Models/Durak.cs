using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Collections.Generic;

namespace WpfAppProlab1.Models
{
    public class Durak
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("lat")]
        public double Enlem { get; set; }

        [JsonProperty("lon")]
        public double Boylam { get; set; }

        [JsonProperty("sonDurak")]
        public bool SonDurak { get; set; }

        [JsonProperty("nextStops")]
        public List<SonrakiDurak> SonrakiDuraklar { get; set; } = new List<SonrakiDurak>();

        [JsonProperty("transfer")]
        public Aktarma Aktar { get; set; }

        public override string ToString() => Name;
    }
}

