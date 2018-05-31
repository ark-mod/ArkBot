using ArkBot.Configuration.Model;
using ArkBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    public static class ArkDataHelper
    {
        public static double? CalculateMaxStat(ArkSpeciesStatsData.Stat stat, string speciesNameOrClass, int? wildLevelStat, int? tamedLevelStat, decimal? imprintingQuality, decimal? tamedIneffectivenessModifier)
        {
            var speciesAliases = ArkSpeciesAliases.Instance.GetAliases(speciesNameOrClass) ?? new[] { speciesNameOrClass };
            return ArkSpeciesStats.Instance.Data?.GetMaxValue(
                            speciesAliases, //a list of alternative species names
                            stat,
                            wildLevelStat ?? 0,
                            tamedLevelStat ?? 0,
                            (double)(1 / (1 + (tamedIneffectivenessModifier ?? 0m))),
                            (double)(imprintingQuality ?? 0m));
        }

        public static double? CalculateBabyFullyGrown(string speciesNameOrClass, float babyAge, IConfig config)
        {
            var speciesAliases = ArkSpeciesAliases.Instance.GetAliases(speciesNameOrClass) ?? new[] { speciesNameOrClass };
            var data = ArkSpeciesStats.Instance.Data?.GetSpecies(speciesAliases);
            if (data == null) return null;

            var adj = data.Breeding.GetAdjusted(config.ArkMultipliers);

            var remaining = (1.0f - babyAge) * adj.MaturationTime;

            return remaining;
        }
    }
}
