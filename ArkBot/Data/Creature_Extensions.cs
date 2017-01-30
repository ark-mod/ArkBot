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
    }
}
