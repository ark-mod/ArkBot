using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
