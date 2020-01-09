using ArkBot.Extensions;
using System;
using System.Linq;

namespace ArkBot.Database.Model
{
    // IMPORTANT NOTE
    // All types that inherit from Vote should configure TPT (Table per Type) inheritance mapping strategy in EfDatabaseContext
    // IE. modelBuilder.Entity<...>().ToTable("<name>");
    public class UpdateServerVote : Vote
    {
        public override string ToString()
        {
            var remaining = Finished - DateTime.Now;
            return $@"Vote to update server due to ""{Reason}""{Environment.NewLine}{Votes.Count} votes ({Votes.Count(x => x.VotedFor)} voted for, {Votes.Count(x => !x.VotedFor)} voted against)" + (remaining > TimeSpan.Zero ? ", remaining time " + remaining.ToStringCustom(true) : "");
        }
    }
}
