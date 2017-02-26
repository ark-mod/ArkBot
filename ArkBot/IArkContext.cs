using System;
using System.Threading.Tasks;
using ArkBot.Data;

namespace ArkBot
{
    public interface IArkContext: IDisposable
    {
        IProgress<string> Progress { get; }
        TimeSpan? ApproxTimeUntilNextUpdate { get; }
        ArkSpeciesStatsData ArkSpeciesStatsData { get; set; }
        CreatureClass[] Classes { get; }
        Cluster Cluster { get; }
        Creature[] Creatures { get; }
        Creature[] Wild { get; }
        DateTime LastUpdate { get; }
        Player[] Players { get; }
        ArkSpeciesAliases SpeciesAliases { get; set; }
        Tribe[] Tribes { get; }
        Task Initialize(ArkSpeciesAliases aliases = null);
        double? CalculateMaxFood(string speciesNameOrClass, int? wildLevelFood, int? tamedLevelFood, decimal? imprintingQuality);
        string GetElevationAsText(decimal z);
    }
}