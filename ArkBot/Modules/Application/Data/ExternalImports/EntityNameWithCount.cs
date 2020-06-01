using Newtonsoft.Json;

namespace ArkBot.Modules.Application.Data.ExternalImports
{
    public class EntityNameWithCount
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "count")]
        public long Count { get; set; }
    }
}
