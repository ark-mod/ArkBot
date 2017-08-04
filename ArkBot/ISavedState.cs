using System;
using System.Collections.Generic;

namespace ArkBot
{
    public interface ISavedState
    {
        int LatestTribeLogDay { get; set; }
        TimeSpan LatestTribeLogTime { get; set; }
        bool VotingDisabled { get; set; }
        bool SkipExtractNextRestart { get; set; }
        List<PlayerLastActiveSavedState> PlayerLastActive { get; set; }
        bool Save();
    }
}