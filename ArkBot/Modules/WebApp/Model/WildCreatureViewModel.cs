using Newtonsoft.Json;

namespace ArkBot.Modules.WebApp.Model
{
    public class WildCreatureViewModel
    {
        public WildCreatureViewModel()
        {
            Aliases = new string[] { };
        }

        public long Id1 { get; set; }
        public long Id2 { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ClassName { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Species { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string[] Aliases { get; set; }
        public string Gender { get; set; }
        public int BaseLevel { get; set; }
        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Z { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? TopoMapX { get; set; }
        public float? TopoMapY { get; set; }
        public bool IsTameable { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public CreatureStatsViewModel BaseStats { get; set; }

    }
}