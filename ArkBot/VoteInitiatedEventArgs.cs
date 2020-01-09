using System;

namespace ArkBot
{
    public class VoteInitiatedEventArgs : EventArgs
    {
        public Database.Model.Vote Item { get; set; }
    }
}
