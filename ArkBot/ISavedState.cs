using System;

namespace ArkBot
{
    public interface ISavedState
    {
        int LatestTribeLogDay { get; set; }
        TimeSpan LatestTribeLogTime { get; set; }
        bool VotingDisabled { get; set; }
        bool Save();
    }
}