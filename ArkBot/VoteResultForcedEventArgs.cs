using ArkBot.Database.Model;
using System;

namespace ArkBot
{
    public class VoteResultForcedEventArgs : EventArgs
    {
        public Database.Model.Vote Item { get; set; }
        public VoteResult Result { get; set; }
    }
}
