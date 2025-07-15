using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Collections.Generic;

namespace WpfAppProlab1.Models
{
    public class SehirVerisi
    {
        [JsonProperty("city")]
        public string Sehir { get; set; }

        [JsonProperty("taxi")]
        public Taxi Taksi { get; set; }

        [JsonProperty("duraklar")]
        public List<Durak> Duraklar { get; set; }
    }
}

