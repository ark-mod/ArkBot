using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class WildCreatureStatistics
    {
        public WildCreatureStatistics()
        {
            Species = new List<WildCreatureSpeciesStatistics>();
        }

        public int CreatureCount { get; set; }
        public List<WildCreatureSpeciesStatistics> Species { get; set; }
    }
}
