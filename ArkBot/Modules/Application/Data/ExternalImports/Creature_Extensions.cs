using Newtonsoft.Json;

namespace ArkBot.Modules.Application.Data.ExternalImports
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
