using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Data
{
    public partial class Tribe
    {
        [JsonIgnore]
        [JsonProperty(PropertyName = "id")]
        public int? Id { get; set; }
    }
}
