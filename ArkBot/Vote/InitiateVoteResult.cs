using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Vote
{
    public class InitiateVoteResult : VoteStateChangeResult
    {
        public string MessageInitiator { get; set; }
        public Database.Model.Vote Vote { get; set; }
    }
}
