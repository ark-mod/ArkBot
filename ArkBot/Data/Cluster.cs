using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Data
{
    public class Cluster
    {
        [JsonProperty(PropertyName = "creatures")]
        public Creature[] Creatures { get; set; }
    }
}
