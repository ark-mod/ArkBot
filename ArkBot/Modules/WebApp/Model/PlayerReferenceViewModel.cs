using Newtonsoft.Json;
using System;

namespace ArkBot.Modules.WebApp.Model
{
    public class PlayerReferenceViewModel
    {
        public int Id { get; set; }
        public string SteamId { get; set; }
        public string SteamName { get; set; }
        public string CharacterName { get; set; }
        public string TribeName { get; set; }
        public int? TribeId { get; set; }
        public DateTime LastActiveTime { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FakeSteamId { get; set; }
    }
}
