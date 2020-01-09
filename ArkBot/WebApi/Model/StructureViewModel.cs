using Newtonsoft.Json;

namespace ArkBot.WebApi.Model
{
    public class StructureViewModel
    {
        [JsonProperty("t")]
        public int TypeId { get; set; }
        [JsonProperty("c")]
        public int Count { get; set; }
    }
}
