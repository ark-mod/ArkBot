using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Data
{
    public partial class Creature
    {
        [JsonProperty(PropertyName = "speciesClass")]
        public string SpeciesClass { get; set; }

        [JsonProperty(PropertyName = "speciesName")]
        public string SpeciesName { get; set; }

        [JsonIgnore]
        public double TamingEffectiveness => (double)(1 / (1 + (TamedIneffectivenessModifier ?? 0m)));

        [JsonIgnore]
        public bool IsInCluster { get; set; }
    }
}
