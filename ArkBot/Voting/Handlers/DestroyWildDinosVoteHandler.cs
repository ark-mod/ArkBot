using ArkBot.Ark;
using ArkBot.Configuration.Model;
using ArkBot.Database;
using ArkBot.Database.Model;
using ArkBot.Extensions;
using ArkBot.Helpers;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArkBot.Voting.Handlers
{
    public class DestroyWildDinosVoteHandler : IVoteHandler<DestroyWildDinosVote>
    {
        private DestroyWildDinosVote _vote;

        public DestroyWildDinosVoteHandler(Database.Model.Vote vote)
        {
            _vote = vote as DestroyWildDinosVote;
        }

        public static async Task<InitiateVoteResult> Initiate(IMessageChannel channel, ArkServerContext context, IConfig config, IEfDatabaseContext db, ulong userId, string identifier, DateTime when, string reason)
        {
            var votes = db.Votes.OfType<DestroyWildDinosVote>().Where(x => x.Result == VoteResult.Undecided).ToArray();
            if (votes.Length > 0)
            {
                await channel.SendMessageDirectedAt(userId, $"there is already an active vote to wipe wild dinos.");
                return null;
            }

            //proceed to initiate vote
            var vote = new DestroyWildDinosVote
            {
                Reason = reason,
                Started = when,
#if DEBUG
                Finished = when.AddSeconds(10),
#else
                Finished = when.AddMinutes(5),
#endif
                Result = VoteResult.Undecided,
                ServerKey = context.Config.Key,
                Identifier = identifier
            };

            return new InitiateVoteResult
            {
                MessageInitiator = $"the vote to wipe wild dinos have been initiated. Announcement will be made.",
                MessageAnnouncement = $@"**A vote to wipe wild dinos on server ({context.Config.Key}) due to ""{reason}"" have been started. Please cast your vote in the next five minutes!**{Environment.NewLine}To vote use the command: **!vote {identifier} yes**/**no**",
                MessageRcon = $@"A vote to wipe wild dinos due to ""{reason}"" have been started. Please cast your vote on Discord using !vote {identifier} yes/no in the next five minutes!",
                Vote = vote
            };
        }

        public VoteStateChangeResult VoteIsAboutToExpire()
        {
            if (_vote == null) return null;

            return new VoteStateChangeResult
            {
                MessageAnnouncement = $@"**Vote to wipe wild dinos on server ({_vote.ServerKey}) have one minute remaining...**",
                MessageRcon = $@"Vote to wipe wild dinos have one minute remaining..."
            };
        }


        public async Task<VoteStateChangeResult> VoteFinished(ArkServerContext serverContext, IConfig config, IConstants constants, IEfDatabaseContext db)
        {
            if (_vote == null) return null;
            if (serverContext == null) return null;

            if (_vote.Result == VoteResult.Passed)
            {
                await serverContext.Steam.SendRconCommand($"destroywilddinos");
            }

            return new VoteStateChangeResult
            {
                MessageAnnouncement = $@"{(_vote.Result == VoteResult.Passed ? ":white_check_mark:" : ":x:")} **Vote to wipe wild dinos on server ({_vote.ServerKey}) have {(_vote.Result == VoteResult.Vetoed ? "been vetoed" : _vote.Result == VoteResult.Passed ? "passed" : "failed")}**",
                MessageRcon = $@"Vote to wipe wild dinos have {(_vote.Result == VoteResult.Vetoed ? "been vetoed" : _vote.Result == VoteResult.Passed ? "passed" : "failed")}."
            };
        }
    }
}
