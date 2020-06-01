using System.Collections.Generic;

namespace ArkBot.Modules.WebApp.Model
{
    public class WildCreaturesViewModel
    {
        public WildCreaturesViewModel()
        {
            Species = new Dictionary<string, WildCreatureSpeciesViewModel>();
        }

        public WildCreatureStatistics Statistics { get; set; }
        public Dictionary<string, WildCreatureSpeciesViewModel> Species { get; set; }
    }
}
