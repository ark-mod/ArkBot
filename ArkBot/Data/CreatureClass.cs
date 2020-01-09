using Newtonsoft.Json;

namespace ArkBot.Data
{
    public class CreatureClass
    {
        [JsonProperty(PropertyName = "cls")]
        public string Class { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
