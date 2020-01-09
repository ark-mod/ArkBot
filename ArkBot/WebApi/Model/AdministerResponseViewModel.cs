using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ArkBot.WebApi.Model
{
    public class AdministerResponseViewModel
    {
        public string Message { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? DestroyedStructureCount { get; set; }
    }
}
