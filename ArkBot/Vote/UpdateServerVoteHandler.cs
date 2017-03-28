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

namespace ArkBot.Vote
{
    public class UpdateServerVoteHandler : IVoteHandler<UpdateServerVote>
    {
        private UpdateServerVote _vote;
        private IArkServerService _arkServerService;

        public UpdateServerVoteHandler(Database.Model.Vote vote, IArkServerService arkServerService)
        {
            _vote = vote as UpdateServerVote;
            _arkServerService = arkServerService;
        }

        public static async Task<InitiateVoteResult> Initiate(Channel channel, IArkContext context, IConfig config, IEfDatabaseContext db, ulong userId, string identifier, DateTime when, string reason)
        {
            if (db.Votes.OfType<RestartServerVote>().Where(x => x.Result == VoteResult.Undecided).Any() || db.Votes.OfType<UpdateServerVote>().Where(x => x.Result == VoteResult.Undecided).Any())
            {
                await channel.SendMessageDirectedAt(userId, $"there is already an active vote to manage the server.");
                return null;
            }

            //proceed to initiate vote
            var vote = new UpdateServerVote
            {
                Reason = reason,
                Started = when,
                Finished = when.AddMinutes(config.Debug ? 1: 5),
                Result = VoteResult.Undecided,
                Identifier = identifier
            };

            return new InitiateVoteResult
            {
                MessageInitiator = $"the vote to update the server have been initiated. Announcement will be made.",
                MessageAnnouncement = $@"@everyone **A vote to update the server due to ""{reason}"" have been started. Please cast your vote in the next five minutes!**{Environment.NewLine}To vote use the command: **!vote {identifier} yes**/**no**",
                MessageRcon = $@"A vote to update the server due to ""{reason}"" have been started. Please cast your vote on Discord using !vote {identifier} yes/no in the next five minutes!",
                Vote = vote
            };
        }

        public VoteStateChangeResult VoteIsAboutToExpire()
        {
            if (_vote == null) return null;

            return new VoteStateChangeResult
            {
                MessageAnnouncement = $@"**Vote to update the server have one minute remaining...**",
                MessageRcon = $@"Vote to update the server have one minute remaining..."
            };
        }


        public async Task<VoteStateChangeResult> VoteFinished(IConfig config, IConstants constants, IEfDatabaseContext db)
        {
            if (_vote == null) return null;

            return new VoteStateChangeResult
            {
                MessageAnnouncement = $@"{(_vote.Result == VoteResult.Passed ? ":white_check_mark:" : ":x:")} **Vote to update the server have {(_vote.Result == VoteResult.Vetoed ? "been vetoed" : _vote.Result == VoteResult.Passed ? "passed" : "failed")}**",
                MessageRcon = $@"Vote to update the server have {(_vote.Result == VoteResult.Vetoed ? "been vetoed" : _vote.Result == VoteResult.Passed ? "passed" : "failed")}.",
                ReactDelayInMinutes = 5,
                ReactDelayFor = "Server update",
                React = _vote.Result == VoteResult.Passed ? new Func<Task>(async () =>
                {
                    string message = null;
                    if (!await _arkServerService.UpdateServer((s) => { message = s; return Task.FromResult((Message)null); }, (s) => s.FirstCharToUpper()))
                    {
                        Logging.Log($@"Vote to update server failed (""{message ?? ""}"")", GetType(), LogLevel.DEBUG);
                    }
                }) : null
            };
        }
    }
}
