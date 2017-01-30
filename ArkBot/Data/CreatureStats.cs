using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Data
{
    public class CreatureStats
    {
        [JsonProperty(PropertyName = "health")]
        public int? Health { get; set; }
        [JsonProperty(PropertyName = "stamina")]
        public int? Stamina { get; set; }
        [JsonProperty(PropertyName = "oxygen")]
        public int? Oxygen { get; set; }
        [JsonProperty(PropertyName = "food")]
        public int? Food { get; set; }
        [JsonProperty(PropertyName = "weight")]
        public int? Weight { get; set; }
        [JsonProperty(PropertyName = "melee")]
        public int? MeleeDamage { get; set; }
        [JsonProperty(PropertyName = "speed")]
        public int? Speed { get; set; }
    }
}
