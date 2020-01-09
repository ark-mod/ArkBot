using System.Collections.Generic;

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
