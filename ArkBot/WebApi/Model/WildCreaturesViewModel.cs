using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
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
