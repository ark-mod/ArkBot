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
    public class SetTimeOfDayVoteHandler : IVoteHandler<SetTimeOfDayVote>
    {
        private SetTimeOfDayVote _vote;

        public SetTimeOfDayVoteHandler(Database.Model.Vote vote)
        {
            _vote = vote as SetTimeOfDayVote;
        }

        public static async Task<InitiateVoteResult> Initiate(IMessageChannel channel, ArkServerContext context, IConfig config, IEfDatabaseContext db, ulong userId, string identifier, DateTime when, string reason, string timeOfDayRaw)
        {
            var _rTimeOfDay = new Regex(@"^\s*\d{2,2}\:\d{2,2}(\:\d{2,2})?\s*$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (!_rTimeOfDay.IsMatch(timeOfDayRaw))
            {
                await channel.SendMessageDirectedAt(userId, $"time of day format is invalid.");
                return null;
            }
            
            var votes = db.Votes.OfType<SetTimeOfDayVote>().Where(x => x.Result == VoteResult.Undecided).ToArray();
            if (votes.Length > 0)
            {
                await channel.SendMessageDirectedAt(userId, $"there is already an active vote to set the time of day.");
                return null;
            }

            //proceed to initiate vote
            var vote = new SetTimeOfDayVote
            {
                TimeOfDay = timeOfDayRaw.Trim(),
                Reason = reason,
                Started = when,
#if DEBUG
                Finished = when.AddSeconds(10),
#else
                Finished = when.AddMinutes(2),
#endif
                Result = VoteResult.Undecided,
                ServerKey = context.Config.Key,
                Identifier = identifier
            };

            return new InitiateVoteResult
            {
                MessageInitiator = $"the vote to set time of day to {vote.TimeOfDay} have been initiated. Announcement will be made.",
                MessageAnnouncement = $@"**A vote to set time of day on server ({context.Config.Key}) to {vote.TimeOfDay} due to ""{reason}"" have been started. Please cast your vote in the next two minutes!**{Environment.NewLine}To vote use the command: **!vote {identifier} yes**/**no**",
                MessageRcon = $@"A vote to set time of day to {vote.TimeOfDay} due to ""{reason}"" have been started. Please cast your vote on Discord using !vote {identifier} yes/no in the next two minutes!",
                Vote = vote
            };
        }

        public VoteStateChangeResult VoteIsAboutToExpire()
        {
            if (_vote == null) return null;

            return new VoteStateChangeResult
            {
                MessageAnnouncement = $@"**Vote to set time of day on server ({_vote.ServerKey}) to {_vote.TimeOfDay} have one minute remaining...**",
                MessageRcon = $@"Vote to set time of day to {_vote.TimeOfDay} have one minute remaining..."
            };
        }


        public async Task<VoteStateChangeResult> VoteFinished(ArkServerContext serverContext, IConfig config, IConstants constants, IEfDatabaseContext db)
        {
            if (_vote == null) return null;

            if (_vote.Result == VoteResult.Passed)
            {
                await serverContext.Steam.SendRconCommand($"settimeofday {_vote.TimeOfDay}");
            }

            return new VoteStateChangeResult
            {
                MessageAnnouncement = $@"{(_vote.Result == VoteResult.Passed ? ":white_check_mark:" : ":x:")} **Vote to set time of day on server ({_vote.ServerKey}) to {_vote.TimeOfDay} have {(_vote.Result == VoteResult.Vetoed ? "been vetoed" : _vote.Result == VoteResult.Passed ? "passed" : "failed")}**",
                MessageRcon = $@"Vote to set time of day to {_vote.TimeOfDay} have {(_vote.Result == VoteResult.Vetoed ? "been vetoed" : _vote.Result == VoteResult.Passed ? "passed" : "failed")}."
            };
        }
    }
}
