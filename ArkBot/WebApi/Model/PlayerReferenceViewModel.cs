using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
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
