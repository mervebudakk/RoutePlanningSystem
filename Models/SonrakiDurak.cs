using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WpfAppProlab1.Models
{
    public class SonrakiDurak
    {
        [JsonProperty("stopId")]
        public string stopId { get; set; }

        [JsonProperty("mesafe")]
        public double Mesafe { get; set; }

        [JsonProperty("sure")]
        public int Sure { get; set; }

        [JsonProperty("ucret")]
        public double Ucret { get; set; }
    }
}

