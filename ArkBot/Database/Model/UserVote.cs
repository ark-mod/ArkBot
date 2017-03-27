using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Database.Model
{
    public enum UserVoteType { NewMember, Member, Trusted, Admin }

    public class UserVote
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int VoteId { get; set; }
        public virtual Vote Vote { get; set; }
        public UserVoteType VoteType { get; set; }
        public bool InitiatedVote { get; set; }
        public bool VotedFor { get; set; }
        public bool Vetoed { get; set; }
        public string Reason { get; set; }
        public DateTime When { get; set; }
    }
}
