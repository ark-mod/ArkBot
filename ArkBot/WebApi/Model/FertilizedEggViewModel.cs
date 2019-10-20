using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class FertilizedEggViewModel
    {
        public string CharacterBP { get; set; }
        public string Dino { get; set; }
        public int? EggLevel { get; set; }
        public string SpoilTime { get; set; }
        public string DroppedBySteamId { get; set; }
        public string DroppedBy { get; set; }
    }
}
