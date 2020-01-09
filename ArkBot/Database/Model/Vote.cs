using System;
using System.Collections.Generic;

namespace ArkBot.Database.Model
{
    public enum VoteResult { Undecided, Passed, Failed, Vetoed }

    public abstract class Vote
    {
        public Vote()
        {
            Votes = new List<UserVote>();
        }

        public int Id { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public VoteResult Result { get; set; }
        public string ServerKey { get; set; }
        public string Identifier { get; set; }
        public string Reason { get; set; }
        public virtual ICollection<UserVote> Votes { get; set; }
    }
}
