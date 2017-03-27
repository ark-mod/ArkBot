using ArkBot.Database.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class VoteResultForcedEventArgs : EventArgs
    {
        public Database.Model.Vote Item { get; set; }
        public VoteResult Result { get; set; }
    }
}
