using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Data
{
    public class Player
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "playerName")]
        public string PlayerName { get; set; }
        [JsonProperty(PropertyName = "steamId")]
        public string SteamId { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "level")]
        public int Level { get; set; }
        [JsonProperty(PropertyName = "experience")]
        public decimal Experience { get; set; }
        [JsonIgnore]
        [JsonProperty(PropertyName = "engrams")]
        public string[] Engrams { get; set; }
        [JsonIgnore]
        [JsonProperty(PropertyName = "attributes")]
        public Dictionary<string, int> Attributes { get; set; }
        [JsonProperty(PropertyName = "inventory")]
        public EntityNameWithCount[] Inventory { get; set; }
        [JsonProperty(PropertyName = "x")]
        public decimal? X { get; set; }
        [JsonProperty(PropertyName = "y")]
        public decimal? Y { get; set; }
        [JsonProperty(PropertyName = "z")]
        public decimal? Z { get; set; }
        [JsonProperty(PropertyName = "lat")]
        public decimal? Latitude { get; set; }
        [JsonProperty(PropertyName = "lon")]
        public decimal? Longitude { get; set; }
        [JsonProperty(PropertyName = "tribeId")]
        public int? TribeId { get; set; }
        [JsonProperty(PropertyName = "tribeName")]
        public string TribeName { get; set; }
    }
}
