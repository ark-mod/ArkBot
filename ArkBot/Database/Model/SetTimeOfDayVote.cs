using ArkBot.Extensions;
using System;
using System.Linq;

namespace ArkBot.Database.Model
{
    // IMPORTANT NOTE
    // All types that inherit from Vote should configure TPT (Table per Type) inheritance mapping strategy in EfDatabaseContext
    // IE. modelBuilder.Entity<...>().ToTable("<name>");
    public class SetTimeOfDayVote : Vote
    {
        public string TimeOfDay { get; set; }

        public override string ToString()
        {
            var remaining = Finished - DateTime.Now;
            return $@"Vote to set time of day to {TimeOfDay} due to ""{Reason}""{Environment.NewLine}{Votes.Count} votes ({Votes.Count(x => x.VotedFor)} voted for, {Votes.Count(x => !x.VotedFor)} voted against)" + (remaining > TimeSpan.Zero ? ", remaining time " + remaining.ToStringCustom(true) : "");
        }
    }
}
