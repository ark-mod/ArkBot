using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class PlayerServerViewModel
    {
        public PlayerServerViewModel()
        {
            Creatures = new List<TamedCreatureViewModel>();
            //KibblesAndEggs = new List<KibbleAndEggViewModel>();
            //CropPlots = new List<CropPlotViewModel>();
            //ElectricalGenerators = new List<ElectricalGeneratorViewModel>();
        }

        public string ClusterKey { get; set; }
        public string SteamId { get; set; }
        public string CharacterName { get; set; }
        public string Gender { get; set; }
        public string TribeName { get; set; }
        public int? TribeId { get; set; }
        public int? Level { get; set; }
        public int? EngramPoints { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? TopoMapX { get; set; }
        public float? TopoMapY { get; set; }
        public DateTime SavedAt { get; set; }
        public List<TamedCreatureViewModel> Creatures { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FakeSteamId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<KibbleAndEggViewModel> KibblesAndEggs { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<CropPlotViewModel> CropPlots { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ElectricalGeneratorViewModel> ElectricalGenerators { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<TribeLogEntryViewModel> TribeLog { get; set; }
    }
}
