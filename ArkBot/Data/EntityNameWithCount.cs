using Newtonsoft.Json;

namespace ArkBot.Data
{
    public class EntityNameWithCount
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "count")]
        public long Count { get; set; }
    }
}
