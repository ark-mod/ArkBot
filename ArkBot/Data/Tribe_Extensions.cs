using Newtonsoft.Json;

namespace ArkBot.Data
{
    public partial class Tribe
    {
        [JsonIgnore]
        [JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }
    }
}
