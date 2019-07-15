using Newtonsoft.Json;
using System.Collections.Generic;

namespace ArkBot.WebApi.Model
{
    public class FertilizedEggsResponseViewModel
    {
        public string Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? FertilizedEggsCount { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? SpoiledFertilizedEggsCount { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<FertilizedEggViewModel> FertilizedEggList { get; set; }
    }
}
