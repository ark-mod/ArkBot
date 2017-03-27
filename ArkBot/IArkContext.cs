using System;
using System.Threading.Tasks;
using ArkBot.Data;
using System.Collections.Generic;
using ArkBot.Database.Model;

namespace ArkBot
{
    public delegate void VoteInitiatedEventHandler(object sender, VoteInitiatedEventArgs e);
    public delegate void VoteResultForcedEventHandler(object sender, VoteResultForcedEventArgs e);

    public interface IArkContext: IDisposable
    {
        IProgress<string> Progress { get; }
        TimeSpan? ApproxTimeUntilNextUpdate { get; }
        ArkSpeciesStatsData ArkSpeciesStatsData { get; set; }
        CreatureClass[] Classes { get; }
        Cluster Cluster { get; }
        Creature[] Creatures { get; }
        IEnumerable<Creature> CreaturesNoRaft { get; }
        IEnumerable<Creature> CreaturesInclCluster { get; }
        IEnumerable<Creature> CreaturesInclClusterNoRaft { get; }
        Creature[] Wild { get; }
        DateTime LastUpdate { get; }
        Player[] Players { get; }
        ArkSpeciesAliases SpeciesAliases { get; set; }
        Tribe[] Tribes { get; }
        Task Initialize(ArkSpeciesAliases aliases = null);
        double? CalculateMaxStat(ArkSpeciesStatsData.Stat stat, string speciesNameOrClass, int? wildLevelStat, int? tamedLevelStat, decimal? imprintingQuality, decimal? tamedIneffectivenessModifier);
        string GetElevationAsText(decimal z);
        event ArkContext.ContextUpdated Updated;
        void DebugTriggerOnChange();
        void OnVoteInitiated(Database.Model.Vote item);
        void OnVoteResultForced(Database.Model.Vote item, VoteResult forcedResult);

        event VoteInitiatedEventHandler VoteInitiated;
        event VoteResultForcedEventHandler VoteResultForced;
    }
}