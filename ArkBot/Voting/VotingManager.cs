using ArkBot.Ark;
using ArkBot.Configuration.Model;
using ArkBot.Database;
using ArkBot.Database.Model;
using ArkBot.Discord;
using ArkBot.Extensions;
using ArkBot.Helpers;
using ArkBot.ScheduledTasks;
using ArkBot.Voting.Handlers;
using Autofac;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Voting
{
    public class VotingManager
    {
        private ArkContextManager _contextManager;
        private DiscordManager _discordManager;
        private ScheduledTasksManager _scheduledTasksManager;
        private EfDatabaseContextFactory _databaseContextFactory;
        private IConfig _config;
        private IConstants _constants;
        private ILifetimeScope _scope;
        private IProgress<string> _progress;

        public VotingManager(
            ArkContextManager contextManager, 
            DiscordManager discordManager,
            ScheduledTasksManager scheduledTasksManager,
            EfDatabaseContextFactory databaseContextFactory, 
            IConfig config, 
            IConstants constants, 
            ILifetimeScope scope, 
            IProgress<string> progress)
        {
            _contextManager = contextManager;
            _discordManager = discordManager;
            _scheduledTasksManager = scheduledTasksManager;
            _databaseContextFactory = databaseContextFactory;
            _config = config;
            _constants = constants;
            _scope = scope;
            _progress = progress;

            _contextManager.VoteInitiated += _contextManager_VoteInitiated;
            _contextManager.VoteResultForced += _contextManager_VoteResultForced;
            _contextManager.InitializationCompleted += _contextManager_InitializationCompleted;
        }

        public async Task TimerUpdateVotes()
        {
            using (var db = _databaseContextFactory.Create())
            {
                var elapsedBans = db.Votes.OfType<BanVote>().Where(x => x.Result == VoteResult.Passed && x.BannedUntil.HasValue && x.BannedUntil <= DateTime.Now).ToArray();
                foreach (var ban in elapsedBans)
                {
                    var serverContext = _contextManager.GetServer(ban.ServerKey);
                    if (serverContext == null)
                    {
                        Logging.Log($"Failed to automatically unban player {ban.SteamId} on server ({ban.ServerKey}) because the server was not found.", typeof(ArkDiscordBot), LogLevel.WARN);
                        continue;
                    }

                    if (await serverContext.Steam.SendRconCommand($"unbanplayer {ban.SteamId}") != null) ban.BannedUntil = null;
                }

                db.SaveChanges();
            }
        }
        
        private async void _contextManager_InitializationCompleted()
        {
            //handle undecided votes (may happen due to previous bot shutdown before vote finished)
            using (var db = _databaseContextFactory.Create())
            {
                var votes = db.Votes.Where(x => x.Result == VoteResult.Undecided);
                foreach (var vote in votes)
                {
                    var serverContext = _contextManager.GetServer(vote.ServerKey);
                    if (serverContext == null)
                    {
                        Logging.Log($"Failed to re-setup vote on server {vote.ServerKey} with key {vote.Identifier} and id {vote.Id}", typeof(ArkDiscordBot), LogLevel.WARN);
                        continue;
                    }

                    if (DateTime.Now >= vote.Finished)
                    {
                        await VoteFinished(serverContext, db, vote, true);
                    }
                    else
                    {
                        _contextManager_VoteInitiated(serverContext, new VoteInitiatedEventArgs { Item = vote });
                    }
                }
            }
        }

        private async void _contextManager_VoteResultForced(ArkServerContext sender, VoteResultForcedEventArgs args)
        {
            if (args == null || args.Item == null) return;

            _scheduledTasksManager.RemoveTimedTaskByTag("vote_" + args.Item.Id);

            using (var db = _databaseContextFactory.Create())
            {
                var vote = db.Votes.FirstOrDefault(x => x.Id == args.Item.Id);

                await VoteFinished(sender, db, vote, forcedResult: args.Result);
            }
        }

        private IVoteHandler GetVoteHandler(Database.Model.Vote vote)
        {
            IVoteHandler handler = null;
            var type = ObjectContext.GetObjectType(vote.GetType());
            try
            {
                handler = _scope.Resolve(typeof(IVoteHandler<>).MakeGenericType(type), new TypedParameter(typeof(Database.Model.Vote), vote)) as IVoteHandler;
            }
            catch (Exception ex)
            {
                Logging.LogException($"Failed to resolve IVoteHandler for type '{type.Name}'", ex, GetType(), LogLevel.ERROR, ExceptionLevel.Unhandled);
                return null;
            }

            if (handler == null)
            {
                Logging.Log($"Failed to resolve IVoteHandler for type '{type.Name}'", GetType(), LogLevel.ERROR);
                return null;
            }

            return handler;
        }

        private void _contextManager_VoteInitiated(ArkServerContext serverContext, VoteInitiatedEventArgs args)
        {
            if (args == null || args.Item == null) return;

            var handler = GetVoteHandler(args.Item);
            if (handler == null) return;

            //reminder, one minute before expiry
            var reminderAt = args.Item.Finished.AddMinutes(-1);
            if (DateTime.Now < reminderAt)
            {
                _scheduledTasksManager.AddTimedTask(new TimedTask
                {
                    When = reminderAt,
                    Tag = "vote_" + args.Item.Id,
                    Callback = new Func<Task>(async () =>
                    {
                        var result = handler.VoteIsAboutToExpire();
                        if (result == null) return;

                        if (result.MessageRcon != null) await serverContext.Steam.SendRconCommand($"serverchat {result.MessageRcon.ReplaceRconSpecialChars()}");
                        if (result.MessageAnnouncement != null && !string.IsNullOrWhiteSpace(_config.Discord.AnnouncementChannel))
                        {
                            await _discordManager.SendTextMessageToChannelNameOnAllServers(_config.Discord.AnnouncementChannel, result.MessageAnnouncement);
                        }
                    })
                });
            }

            //on elapsed
            _scheduledTasksManager.AddTimedTask(new TimedTask
            {
                When = args.Item.Finished,
                Tag = "vote_" + args.Item.Id,
                Callback = new Func<Task>(async () =>
                {
                    using (var db = _databaseContextFactory.Create())
                    {
                        var vote = db.Votes.FirstOrDefault(x => x.Id == args.Item.Id);

                        await VoteFinished(serverContext, db, vote);
                    }
                })
            });
        }

        private async Task VoteFinished(ArkServerContext serverContext, IEfDatabaseContext db, Database.Model.Vote vote, bool noAnnouncement = false, VoteResult? forcedResult = null)
        {
            var handler = GetVoteHandler(vote);
            if (handler == null) return;

            var votesFor = vote.Votes.Count(x => x.VotedFor);
            var votesAgainst = vote.Votes.Count(x => !x.VotedFor);
#if DEBUG
            vote.Result = forcedResult ?? (vote.Votes.Count >= 1 && votesFor > votesAgainst ? VoteResult.Passed : VoteResult.Failed);
#else
            vote.Result = forcedResult ?? (vote.Votes.Count >= 3 && votesFor > votesAgainst ? VoteResult.Passed : VoteResult.Failed);
#endif

            VoteStateChangeResult result = null;
            try
            {
                result = await handler.VoteFinished(serverContext, _config, _constants, db);
                try
                {
                    if (!noAnnouncement && result != null)
                    {
                        if (result.MessageRcon != null) await serverContext.Steam.SendRconCommand($"serverchat {result.MessageRcon.ReplaceRconSpecialChars()}");
                        if (result.MessageAnnouncement != null && !string.IsNullOrWhiteSpace(_config.Discord.AnnouncementChannel))
                        {
                            await _discordManager.SendTextMessageToChannelNameOnAllServers(_config.Discord.AnnouncementChannel, result.MessageAnnouncement);
                        }
                    }
                }
                catch { /* ignore all exceptions */ }

                if (result != null && result.React != null)
                {
                    if (result.ReactDelayInMinutes <= 0) await result.React();
                    else
                    {
                        await _scheduledTasksManager.StartCountdown(serverContext, result.ReactDelayFor, result.ReactDelayInMinutes, result.React);
                    }
                }
            }
            catch (Exception ex)
            {
                //todo: better exception handling structure
                Logging.LogException(ex.Message, ex, GetType(), LogLevel.ERROR, ExceptionLevel.Unhandled);
            }
            db.SaveChanges();

        }
    }
}
