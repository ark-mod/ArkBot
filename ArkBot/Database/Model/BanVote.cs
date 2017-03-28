using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Extensions;

namespace ArkBot.Database.Model
{
    // IMPORTANT NOTE
    // All types that inherit from Vote should configure TPT (Table per Type) inheritance mapping strategy in EfDatabaseContext
    // IE. modelBuilder.Entity<...>().ToTable("<name>");
    public class BanVote : Vote
    {
        public string PlayerName { get; set; }
        public string CharacterName { get; set; }
        public string TribeName { get; set; }
        public long SteamId { get; set; }
        public DateTime? BannedUntil { get; set; }
        public int DurationInHours { get; set; }

        [NotMapped]
        public string FullName => $"{PlayerName} ({CharacterName}){(!string.IsNullOrWhiteSpace(TribeName) ? $" [{TribeName}]" : "")}";

        public override string ToString()
        {
            var remaining = Finished - DateTime.Now;
            return $@"Vote to ban {FullName}{(DurationInHours <= (24*90) ? $" for {DurationInHours}h" : "")} due to ""{Reason}""{Environment.NewLine}{Votes.Count} votes ({Votes.Count(x => x.VotedFor)} votes for, {Votes.Count(x => !x.VotedFor)} votes against)" + (remaining > TimeSpan.Zero ? ", remaining time " + remaining.ToStringCustom(true) : "");
        }
    }
}
