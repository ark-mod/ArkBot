using Newtonsoft.Json;

namespace ArkBot.Modules.Application.Data.ExternalImports
{
    public partial class Tribe
    {
        [JsonIgnore]
        [JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }
    }
}
