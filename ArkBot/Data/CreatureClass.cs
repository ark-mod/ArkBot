using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Data
{
    public class CreatureClass
    {
        [JsonProperty(PropertyName = "cls")]
        public string Class { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }
}
