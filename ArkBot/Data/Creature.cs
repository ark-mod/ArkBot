using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Data
{
    public partial class TamedCreature : Creature
    {
    }

    public partial class WildCreature : Creature
    {
    }

    public partial class Creature
    {
        public Creature()
        {
            WildLevels = new CreatureStats();
        }

        [JsonProperty(PropertyName = "x")]
        public decimal X { get; set; }
        [JsonProperty(PropertyName = "y")]
        public decimal Y { get; set; }
        [JsonProperty(PropertyName = "z")]
        public decimal Z { get; set; }
        [JsonProperty(PropertyName = "lat")]
        public decimal Latitude { get; set; }
        [JsonProperty(PropertyName = "lon")]
        public decimal Longitude { get; set; }
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }
        [JsonProperty(PropertyName = "tamed")]
        public bool Tamed { get; set; }
        [JsonProperty(PropertyName = "team")]
        public int? Team { get; set; }
        [JsonProperty(PropertyName = "playerId")]
        public int? PlayerId { get; set; }
        [JsonProperty(PropertyName = "female")]
        public bool Female { get; set; }
        [JsonProperty(PropertyName = "color0")]
        public int? Color0 { get; set; }
        [JsonProperty(PropertyName = "color1")]
        public int? color1 { get; set; }
        [JsonProperty(PropertyName = "color2")]
        public int? color2 { get; set; }
        [JsonProperty(PropertyName = "color3")]
        public int? color3 { get; set; }
        [JsonProperty(PropertyName = "color4")]
        public int? color4 { get; set; }
        [JsonProperty(PropertyName = "color5")]
        public int? color5 { get; set; }
        [JsonProperty(PropertyName = "tamedAtTime")]
        public decimal? TamedAtTime { get; set; }
        [JsonProperty(PropertyName = "tamedTime")]
        public decimal? TamedTime { get; set; }
        [JsonProperty(PropertyName = "tribe")]
        public string Tribe { get; set; }
        [JsonProperty(PropertyName = "tamer")]
        public string Tamer { get; set; }
        [JsonProperty(PropertyName = "ownerName")]
        public string OwnerName { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "baseLevel")]
        public int BaseLevel { get; set; }
        [JsonProperty(PropertyName = "wildLevels")]
        public CreatureStats WildLevels { get; set; }
        [JsonProperty(PropertyName = "fullLevel")]
        public int? FullLevel { get; set; }
        [JsonProperty(PropertyName = "tamedLevels")]
        public CreatureStats TamedLevels { get; set; }
        [JsonProperty(PropertyName = "experience")]
        public decimal? Experience { get; set; }
        [JsonProperty(PropertyName = "currentFood")]
        public decimal? CurrentFood { get; set; }
        [JsonProperty(PropertyName = "currentHealth")]
        public decimal? CurrentHealth { get; set; }
        [JsonProperty(PropertyName = "imprintingQuality")]
        public decimal? ImprintingQuality { get; set; }
        [JsonProperty(PropertyName = "tamedIneffectivenessModifier")]
        public decimal? TamedIneffectivenessModifier { get; set; }
        [JsonProperty(PropertyName = "isBaby")]
        public bool IsBaby { get; set; }
        [JsonProperty(PropertyName = "babyAge")]
        public decimal? BabyAge { get; set; }
        [JsonProperty(PropertyName = "babyNextCuddleTime")]
        public decimal? BabyNextCuddleTime { get; set; }

        //currentHealth

        //tamed creatures
        //        {
        //    "x":68889.859375,
        //    "y":298977.9375,
        //    "z":-14090.5927734375,
        //    "lat":87.4,
        //    "lon":58.6,
        //    "id":1540698438178299237,
        //    "tamed":true,
        //    "team":8451966,
        //    "playerId":8451966,
        //    "color0":8,
        //    "color1":37,
        //    "color4":33,
        //    "color5":39,
        //    "tamedAtTime":51946.04924701154,
        //    "tamedTime":170387.87262798846,
        //    "tamer":"Bob",
        //    "ownerName":"Tobbe",
        //    "name":"Mr Harold",
        //    "baseLevel":180,
        //    "wildLevels":{
        //        "health":24,
        //        "stamina":27,
        //        "oxygen":22,
        //        "food":26,
        //        "weight":23,
        //        "melee":32,
        //        "speed":25
        //    },
        //    "fullLevel":222,
        //    "tamedLevels":{
        //        "melee":42
        //    },
        //    "experience":200197.1875,
        //    "imprintingQuality":1.0
        //}
    }
}
