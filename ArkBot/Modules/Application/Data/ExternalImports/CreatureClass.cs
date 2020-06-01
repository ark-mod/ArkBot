using Newtonsoft.Json;

namespace ArkBot.Modules.Application.Data.ExternalImports
{
    public class CreatureClass
    {
        [JsonProperty(PropertyName = "cls")]
        public string Class { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
