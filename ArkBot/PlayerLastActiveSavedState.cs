using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class PlayerLastActiveSavedState
    {
        [JsonProperty(PropertyName = "serverKey")]
        public string ServerKey { get; set; }

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "steamId")]
        public string SteamId { get; set; }

        [JsonProperty(PropertyName = "tribeId")]
        public int? TribeId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "characterName")]
        public string CharacterName { get; set; }

        [JsonProperty(PropertyName = "lastActiveTime")]
        public DateTime LastActiveTime { get; set; }
    }
}
