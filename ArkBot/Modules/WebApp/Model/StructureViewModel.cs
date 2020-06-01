using Newtonsoft.Json;

namespace ArkBot.Modules.WebApp.Model
{
    public class StructureViewModel
    {
        [JsonProperty("t")]
        public int TypeId { get; set; }
        [JsonProperty("c")]
        public int Count { get; set; }
    }
}
