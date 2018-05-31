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
    public class BanVoteHandler : IVoteHandler<BanVote>
    {
        private Database.Model.BanVote _vote;

        public BanVoteHandler(Database.Model.Vote vote)
        {
            _vote = vote as BanVote;
        }

        public static async Task<InitiateVoteResult> Initiate(IMessageChannel channel, ArkServerContext context, IConfig config, IEfDatabaseContext db, ulong userId, string identifier, DateTime when, string reason, string targetRaw, int durationInHours)
        {
            var _rId = new Regex(@"^\s*(id|(steam\s*id))\s*\:\s*(?<id>\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
            var m = _rId.Match(targetRaw);

            var targets = m.Success ?
                context.Players.Where(x => x.Id.ToString().Equals(m.Groups["id"].Value) || x.SteamId.Equals(m.Groups["id"].Value)).ToArray()
                : context.Players.Where(x => (x.Name != null && x.Name.Equals(targetRaw, StringComparison.OrdinalIgnoreCase)) || (x.Name != null && x.Name.Equals(targetRaw, StringComparison.OrdinalIgnoreCase))).ToArray();
            if (targets.Length == 0)
            {
                await channel.SendMessageDirectedAt(userId, $"could not find a player with that name (maybe they have not been saved yet?). Try using their steam id instead.");
                return null;
            }
            if (targets.Length > 1)
            {
                await channel.SendMessageDirectedAt(userId, $"there are more than one player with that name. Try using their steam id instead.");
                return null;
            }
            var target = targets.First();
            var steamId = long.Parse(target.SteamId);
            var votes = db.Votes.OfType<BanVote>().Where(x => x.SteamId == steamId).ToArray();
            if (votes.Any(x => x.BannedUntil.HasValue && x.BannedUntil.Value > when))
            {
                await channel.SendMessageDirectedAt(userId, $"this player is already banned.");
                return null;
            }
            var unvotes = db.Votes.OfType<UnbanVote>().Where(x => x.SteamId == steamId).ToArray();
            if (votes.Any(x => x.Result == VoteResult.Undecided) || unvotes.Any(x => x.Result == VoteResult.Undecided))
            {
                await channel.SendMessageDirectedAt(userId, $"there is already an active vote to ban/unban this player.");
                return null;
            }

            //proceed to initiate vote
            var vote = new BanVote
            {
                SteamId = steamId,
                PlayerName = target.Name,
                CharacterName = target.Name,
                TribeName = target.TribeId.HasValue ? context.Tribes?.FirstOrDefault(x => x.Id == target.TribeId)?.Name : null, //target.TribeName,
                Reason = reason,
                Started = when,
#if DEBUG
                Finished = when.AddSeconds(10),
#else
                Finished = when.AddMinutes(5),
#endif
                DurationInHours = durationInHours,
                Result = VoteResult.Undecided,
                ServerKey = context.Config.Key,
                Identifier = identifier
            };

            return new InitiateVoteResult
            {
                MessageInitiator = $"the vote to ban this player have been initiated. Announcement will be made.",
                MessageAnnouncement = $@"**A vote to ban {vote.FullName}{(vote.DurationInHours <= (24 * 90) ? $" for {vote.DurationInHours}h" : "")} due to ""{reason}"" have been started. Please cast your vote in the next five minutes!**{Environment.NewLine}To vote use the command: **!vote {identifier} yes**/**no**",
                MessageRcon = $@"A vote to ban {vote.FullName}{(vote.DurationInHours <= (24 * 90) ? $" for {vote.DurationInHours}h" : "")} due to ""{reason}"" have been started. Please cast your vote on Discord using !vote {identifier} yes/no in the next five minutes!",
                Vote = vote
            };
        }

        public VoteStateChangeResult VoteIsAboutToExpire()
        {
            if (_vote == null) return null;

            return new VoteStateChangeResult
            {
                MessageAnnouncement = $@"**Vote to ban {_vote.FullName} have one minute remaining...**",
                MessageRcon = $@"Vote to ban {_vote.FullName} have one minute remaining..."
            };
        }


        public async Task<VoteStateChangeResult> VoteFinished(ArkServerContext serverContext, IConfig config, IConstants constants, IEfDatabaseContext db)
        {
            if (_vote == null) return null;

            if (_vote.Result == VoteResult.Passed)
            {
                _vote.BannedUntil = _vote.Started.AddHours(_vote.DurationInHours);

                await serverContext.Steam.SendRconCommand($"banplayer {_vote.SteamId}");
            }

            return new VoteStateChangeResult
            {
                MessageAnnouncement = $@"{(_vote.Result == VoteResult.Passed ? ":white_check_mark:" : ":x:")} **Vote to ban {_vote.FullName} have {(_vote.Result == VoteResult.Vetoed ? "been vetoed" : _vote.Result == VoteResult.Passed ? "passed" : "failed")}**",
                MessageRcon = $@"Vote to ban {_vote.FullName} have {(_vote.Result == VoteResult.Vetoed ? "been vetoed" : _vote.Result == VoteResult.Passed ? "passed" : "failed")}."
            };
        }
    }
}
