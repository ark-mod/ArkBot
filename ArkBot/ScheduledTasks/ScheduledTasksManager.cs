using ArkBot.Ark;
using ArkBot.Configuration.Model;
using ArkBot.Discord;
using ArkBot.Extensions;
using ArkBot.Voting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.ScheduledTasks
{
    /// <summary>
    /// Manages scheduled tasks, reoccuring tasks, tasks that should be run at specific intervals etc.
    /// </summary>
    public class ScheduledTasksManager : IDisposable
    {
        private Timer _timer;
        private ConcurrentDictionary<TimedTask, bool> _timedTasks;
        private DateTime _prevTimedBansUpdate;
        private DateTime _prevTopicUpdate;
        private DateTime _prevServerStatusUpdate;

        private VotingManager _votingManager;
        private ArkContextManager _contextManager;
        private DiscordManager _discordManager;
        private IConfig _config;

        // Required properties due to circular dependency
        public VotingManager VotingManager { get { return _votingManager; } set { _votingManager = value; } }

        public ScheduledTasksManager(
            ArkContextManager contextManager,
            DiscordManager discordManager,
            IConfig config)
        {
            _contextManager = contextManager;
            _discordManager = discordManager;
            _config = config;

            // NOTE that _votingManager have not been set yet
            _contextManager.InitializationCompleted += _contextManager_InitializationCompleted;

            _timedTasks = new ConcurrentDictionary<TimedTask, bool>();
            _timer = new Timer(_timer_Callback, null, Timeout.Infinite, Timeout.Infinite);
        }
        public bool AddTimedTask(TimedTask timedTask)
        {
            return _timedTasks.TryAdd(timedTask, true);
        }

        public void RemoveTimedTaskByTag(string tag)
        {
            var tasks = _timedTasks.Keys.Where(x => x.Tag is string && ((string)x.Tag).Equals(tag, StringComparison.OrdinalIgnoreCase)).ToArray();
            foreach (var task in tasks)
            {
                bool tmp;
                _timedTasks.TryRemove(task, out tmp);
            }
        }

        public async Task StartCountdown(ArkServerContext serverContext, string reason, int delayInMinutes, Func<Task> react = null)
        {
            // collection of servers that should receive notifications via rcon for this countdown event
            var serverContexts = serverContext == null ? _contextManager.Servers.Where(x => !x.Config.DisableChatNotifications).ToArray() : new ArkServerContext[] { serverContext };

            foreach (var sc in serverContexts) await sc.Steam.SendRconCommand($"serverchat Countdown started: {reason} in {delayInMinutes} minute{(delayInMinutes > 1 ? "s" : "")}...");
            if (!string.IsNullOrWhiteSpace(_config.Discord.AnnouncementChannel))
            {
                await _discordManager.SendTextMessageToChannelNameOnAllServers(_config.Discord.AnnouncementChannel, $"**Countdown{(serverContext == null ? "" : $" on server {serverContext.Config.Key}")} started: {reason} in {delayInMinutes} minute{(delayInMinutes > 1 ? "s" : "")}...**");
            }

            foreach (var min in Enumerable.Range(1, delayInMinutes))
            {
                AddTimedTask(new TimedTask
                {
                    When = DateTime.Now.AddMinutes(min),
                    Callback = new Func<Task>(async () =>
                    {
                        var countdown = delayInMinutes - min;
                        foreach (var sc in serverContexts) await sc.Steam.SendRconCommand(countdown > 0 ? $"serverchat {reason} in {countdown} minute{(countdown > 1 ? "s" : "")}..." : $"serverchat {reason}...");
                        if (!string.IsNullOrWhiteSpace(_config.Discord.AnnouncementChannel))
                        {
                            await _discordManager.SendTextMessageToChannelNameOnAllServers(_config.Discord.AnnouncementChannel, countdown > 0 ? $"**{(serverContext == null ? "" : $"{serverContext.Config.Key}: ")}{reason} in {countdown} minute{(countdown > 1 ? "s" : "")}...**" : $"**{(serverContext == null ? "" : $"{serverContext.Config.Key}: ")}{reason}...**");
                        }
                        if (countdown <= 0 && react != null) await react();
                    })
                });
            }
        }

        private void _contextManager_InitializationCompleted()
        {
            _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Main proceedure
        /// </summary>
        private async void _timer_Callback(object state)
        {
            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);

                var tasks = _timedTasks.Keys.Where(x => x.When <= DateTime.Now).ToArray();
                foreach (var task in tasks)
                {
                    bool tmp;
                    _timedTasks.TryRemove(task, out tmp);

                    var fireAndForget = Task.Run(task.Callback); //fire and forget
                }

                if (_config.Discord.InfoTopicChannel != null)
                {
                    if (DateTime.Now - _prevTopicUpdate > TimeSpan.FromSeconds(60))
                    {
                        _prevTopicUpdate = DateTime.Now;
                        var topicUpdateTask = Task.Run(async () =>
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine(@"Type !help to get started | ");

                            var n = 0;
                            foreach (var serverContext in _contextManager.Servers)
                            {
                                var lastUpdate = serverContext.LastUpdate;
                                var nextUpdate = serverContext.ApproxTimeUntilNextUpdate;
                                
                                var nextUpdateTmp = nextUpdate?.ToStringCustom();
                                var nextUpdateString = (nextUpdate.HasValue ? (!string.IsNullOrWhiteSpace(nextUpdateTmp) ? $", Next Update in ~{nextUpdateTmp}" : ", waiting for new update ...") : "");
                                var lastUpdateString = lastUpdate.ToStringWithRelativeDay();
                                sb.Append($@"{serverContext.Config.Key}: Updated {lastUpdateString}{nextUpdateString}");
                                if (++n < _contextManager.Servers.Length) sb.AppendLine(" | ");
                            }

                            try
                            {
                                await _discordManager.EditChannelByNameOnAllServers(_config.Discord.InfoTopicChannel, topic: sb.ToString());
                            }
                            catch (System.Net.Http.HttpRequestException ex) when (ex.Message.IndexOf("Error while copying content to a stream", StringComparison.Ordinal) != -1)
                            {
                                Logging.Log("Error when attempting to change bot info channel topic", GetType());
                            }
                            catch (Exception ex)
                            {
                                Logging.LogException("Error when attempting to change bot info channel topic", ex, GetType(), LogLevel.ERROR, ExceptionLevel.Ignored);
                            }
                        });
                    }
                }

                if (DateTime.Now - _prevServerStatusUpdate > TimeSpan.FromMinutes(2))
                {
                    _prevServerStatusUpdate = DateTime.Now;
                    var banUpdateTask = Task.Run(async () =>
                    {
                        foreach(var server in _contextManager.Servers)
                        {
                            await server.Steam.GetServerStatus();
                        }
                    });
                }

                if (DateTime.Now - _prevTimedBansUpdate > TimeSpan.FromMinutes(5))
                {
                    _prevTimedBansUpdate = DateTime.Now;
                    var banUpdateTask = Task.Run(async () => await _votingManager.TimerUpdateVotes());
                }
            }
            catch (Exception ex)
            {
                Logging.LogException("Unhandled exception in bot timer method", ex, GetType(), LogLevel.ERROR, ExceptionLevel.Unhandled);
            }
            finally
            {
                _timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;

            if (disposing)
            {
                _timer?.Dispose();
            }

            disposedValue = true;
        }
        public void Dispose() { Dispose(true); }
        private bool disposedValue = false;
        #endregion
    }
}
