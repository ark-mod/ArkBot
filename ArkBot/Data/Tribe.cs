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
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "members")]
        public string[] Members { get; set; }
        [JsonProperty(PropertyName = "admins")]
        public string[] Admins { get; set; }
        [JsonProperty(PropertyName = "owner")]
        public string Owner { get; set; }
        [JsonIgnore]
        [JsonProperty(PropertyName = "tribeLog")]
        public string[] TribeLog { get; set; }
        [JsonProperty(PropertyName = "structures")]
        public EntityNameWithCount[] Structures { get; set; }
        [JsonProperty(PropertyName = "items")]
        public EntityNameWithCount[] Items { get; set; }
        [JsonIgnore]
        [JsonProperty(PropertyName = "blueprints")]
        public EntityNameWithCount[] Blueprints { get; set; }
    }
}
