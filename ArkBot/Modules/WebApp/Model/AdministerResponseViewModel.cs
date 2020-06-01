using Newtonsoft.Json;

namespace ArkBot.Modules.WebApp.Model
{
    public class AdministerResponseViewModel
    {
        public string Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? DestroyedStructureCount { get; set; }
    }
}
