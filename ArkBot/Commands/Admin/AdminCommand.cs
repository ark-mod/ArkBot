using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Helpers;
using ArkBot.Extensions;
using System.Text.RegularExpressions;
using Autofac;
using ArkBot.Database;
using Discord;
using ArkBot.Services;
using ArkBot.Ark;
using ArkBot.Discord;
using ArkBot.Discord.Command;
using ArkBot.ScheduledTasks;
using Discord.Commands.Builders;
using RestSharp;
using ArkBot.Configuration.Model;

namespace ArkBot.Commands.Admin
{
    public class AdminCommand : ModuleBase<SocketCommandContext>
    {
        private IConfig _config;
        private IConstants _constants;
        private EfDatabaseContextFactory _databaseContextFactory;
        private ISavedState _savedstate;
        private IArkServerService _arkServerService;
        private ISavegameBackupService _savegameBackupService;
        private ArkContextManager _contextManager;
        private ScheduledTasksManager _scheduledTasksManager;

        public AdminCommand(
            ILifetimeScope scope,
            IConfig config,
            IConstants constants,
            EfDatabaseContextFactory databaseContextFactory,
            ISavedState savedstate,
            IArkServerService arkServerService,
            ISavegameBackupService savegameBackupService,
            ArkContextManager contextManager,
            ScheduledTasksManager scheduledTasksManager)
        {
            _config = config;
            _constants = constants;
            _databaseContextFactory = databaseContextFactory;
            _savedstate = savedstate;
            _arkServerService = arkServerService;
            _savegameBackupService = savegameBackupService;
            _contextManager = contextManager;
            _scheduledTasksManager = scheduledTasksManager;
        }

        [CommandHidden]
        [Command("admin")]
        [Summary("Admin commands to manage the ARK Server (rcon etc.)")]
        [SyntaxHelp(null)]
        [UsageExamples(new[]
        {
            "**<server key> SaveWorld**: Forces the server to save the game world to disk in its current state",
            "**<server key> DestroyWildDinos**: Destroys all untamed creatures on the map (which includes all creatures that are currently being tamed)",
            "**<server key> KickPlayer <steamid>**: Kick a player",
            "**<server key> BanPlayer <steamid>**: Ban a player",
            "**<server key> UnbanPlayer <steamid>**: Unban a player",
            "**<server key> KillPlayer <player id>**: Kill a player",
            //"**<server key> SetVotingAllowed <steamid> true/false**: Set voting allowed/disallowed for a player",
            //"**<server key> EnableVoting true/false**: Enable voting system",
            "**<server key> DoExit**: Shutdown server",
            "**<server key> Broadcast <message>**: Broadcast a message to all players on the server",
            "**<server key> ListPlayers**: List all connected players and their SteamIDs",
            "**<server key> RenamePlayer <old name> NewName <new name>**: Rename a player",
            "**<server key> RenameTribe <old name> NewName <new name>**: Rename a tribe",
            "**<server key> ServerChat <message>**: Sends a chat message to all currently connected players",
            "**<server key> ServerChatTo <steamid> Message <message>**: Sends a direct chat message to the player specified by their int64 encoded steam id",
            "**<server key> ServerChatToPlayer <name> Message <message>**: Sends a direct chat message to the player specified by their in-game player name",
            "**<server key> GetPlayerIDForSteamID <steamid>**: Get player id from given steamid",
            "**<server key> GetSteamIDForPlayerID <player id>**: Get steamid from give player id",
            "**<server key> GetTribeIdPlayerList <tribe id>**: Get a list of players in a tribe",
            "**<server key> SetTimeOfDay hh:mm:ss**: Sets the game world's time of day to the specified time",
            "**<server key> restartserver**: Restart the server.",
            "**<server key> updateserver**: Update the server.",
            "**<server key> startserver**: Start the server.",
            "**<server key> stopserver**: Stop/shutdown the server.",
            "**<server key> terminateserver**: Forcibly shutdown an unresponsive server.",
            "**restartservers**: Restart all servers.",
            "**updateservers**: Update all servers.",
            "**startservers**: Start all servers.",
            "**stopservers**: Stop/shutdown all servers.",
            "**<server key> countdown <minutes> <event description>**: Start a countdown without any action.",
            "**<server key> countdown <minutes> <event description> stopserver**: Start a countdown with subsequent server shutdown.",
            "**<server key> countdown <minutes> <event description> restartserver**: Start a countdown with subsequent server restart.",
            "**<server key> countdown <minutes> <event description> updateserver**: Start a countdown with subsequent server update.",
            "**countdown <minutes> <event description>**: Start a countdown on all servers without any action.",
            "**countdown <minutes> <event description> stopservers**: Start a countdown on all servers with subsequent server shutdown.",
            "**countdown <minutes> <event description> restartservers**: Start a countdown on all servers with subsequent server restart.",
            "**countdown <minutes> <event description> updateservers**: Start a countdown on all servers with subsequent server update.",
            "**<server key> backups**: List backups for the server."
        })]
        [RoleRestrictedPrecondition("admin")]
        public async Task Admin([Remainder] string arguments = null)
        {
            //if (!e.Channel.IsPrivate) return;

            var args = CommandHelper.ParseArgs(arguments, new
            {
                ServerKey = "",
                StartServer = false,
                StartServers = false,
                ShutdownServer = false,
                ShutdownServers = false,
                StopServer = false,
                StopServers = false,
                TerminateServer = false,
                RestartServer = false,
                RestartServers = false,
                UpdateServer = false,
                UpdateServers = false,
                Backups = false,
                SteamId = 0L,
                SaveWorld = false,
                DestroyWildDinos = false,
                //EnableVoting = false,
                //SetVotingAllowed = 0L, //steam id
                KickPlayer = 0L, //steam id
                BanPlayer = 0L, //steam id
                UnbanPlayer = 0L, //steam id
                KillPlayer = 0L, //ark player id
                DoExit = false,
                Broadcast = "",
                ListPlayers = false,
                RenamePlayer = "",
                RenameTribe = "",
                NewName = "",
                ServerChat = "",
                ServerChatTo = 0L, //steam id
                ServerChatToPlayer = "",
                Message = "",
                GetPlayerIDForSteamID = 0L, //steam id
                GetSteamIDForPlayerID = 0L, //ark player id
                GetTribeIdPlayerList = 0L,
                SetTimeOfDay = "",
                Countdown = "",
                True = false,
                False = false
            }, x =>
                x.For(y => y.ServerKey, noPrefix: true)
                .For(y => y.StartServer, flag: true)
                .For(y => y.StartServers, flag: true)
                .For(y => y.ShutdownServer, flag: true)
                .For(y => y.ShutdownServers, flag: true)
                .For(y => y.StopServer, flag: true)
                .For(y => y.StopServers, flag: true)
                .For(y => y.TerminateServer, flag: true)
                .For(y => y.RestartServer, flag: true)
                .For(y => y.RestartServers, flag: true)
                .For(y => y.UpdateServer, flag: true)
                .For(y => y.UpdateServers, flag: true)
                .For(y => y.Backups, flag: true)
                .For(y => y.SaveWorld, flag: true)
                .For(y => y.DestroyWildDinos, flag: true)
                //.For(y => y.EnableVoting, flag: true)
                .For(y => y.DoExit, flag: true)
                .For(y => y.ListPlayers, flag: true)
                .For(y => y.Broadcast, untilNextToken: true)
                .For(y => y.RenamePlayer, untilNextToken: true)
                .For(y => y.RenameTribe, untilNextToken: true)
                .For(y => y.NewName, untilNextToken: true)
                .For(y => y.ServerChat, untilNextToken: true)
                .For(y => y.ServerChatToPlayer, untilNextToken: true)
                .For(y => y.Message, untilNextToken: true)
                .For(y => y.Countdown, untilNextToken: true)
                .For(y => y.True, flag: true)
                .For(y => y.False, flag: true));

            var _rTimeOfDay = new Regex(@"^\s*\d{2,2}\:\d{2,2}(\:\d{2,2})?\s*$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var _rCountdown = new Regex(@"^\s*(?<min>\d+)\s+(?<reason>.+)$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            var sb = new StringBuilder();

            var isCountdown = !string.IsNullOrEmpty(args.Countdown) && _rCountdown.IsMatch(args.Countdown);
            var isMultiServerCommand = args != null && (isCountdown
                || args.StartServer || args.StartServers || args.StopServer || args.StopServers
                || args.ShutdownServer || args.ShutdownServers || args.RestartServer || args.RestartServers
                || args.UpdateServer || args.UpdateServers);

            var serverContext = args?.ServerKey != null ? _contextManager.GetServer(args.ServerKey) : null;
            if (serverContext == null && !isMultiServerCommand)
            {
                await Context.Channel.SendMessageAsync($"**Admin commands need to be prefixed with a valid server instance key.**");
                return;
            }

            // collection of servers that this countdown applies to
            var serverContexts = serverContext == null && !string.IsNullOrWhiteSpace(args.ServerKey) ? null
                : serverContext == null ? _contextManager.Servers.ToArray()
                : new ArkServerContext[] { serverContext };

            if (serverContexts == null)
            {
                await Context.Channel.SendMessageAsync($"**The given server instance key is not valid.**");
                return;
            }

            if (isCountdown)
            {
                var m = _rCountdown.Match(args.Countdown);
                var reason = m.Groups["reason"].Value;
                var delayInMinutes = int.Parse(m.Groups["min"].Value);
                if (delayInMinutes < 1) delayInMinutes = 1;

                Func<Task> react = null;
                if (args.StopServer || args.StopServers || args.ShutdownServer || args.ShutdownServers)
                {
                    react = new Func<Task>(async () =>
                    {
                        var tasks = serverContexts.Select(x => Task.Run(async () =>
                        {
                            string message = null;
                            if (!await _arkServerService.ShutdownServer(x.Config.Key, (s) => { message = s; return Task.FromResult((IUserMessage)null); }))
                            {
                                Logging.Log($@"Countdown to shutdown server ({x.Config.Key}) execution failed (""{message ?? ""}"")", GetType(), LogLevel.DEBUG);
                            }
                        })).ToArray();

                        await Task.WhenAll(tasks);
                    });
                }
                else if (args.UpdateServer || args.UpdateServers)
                {
                    react = new Func<Task>(async () =>
                    {
                        var tasks = serverContexts.Select(x => Task.Run(async () =>
                        {
                            string message = null;
                            if (!await _arkServerService.UpdateServer(x.Config.Key, (s) => { message = s; return Task.FromResult((IUserMessage)null); }, (s) => s.FirstCharToUpper(), 300))
                            {
                                Logging.Log($@"Countdown to update server ({x.Config.Key}) execution failed (""{message ?? ""}"")", GetType(), LogLevel.DEBUG);
                            }
                        })).ToArray();

                        await Task.WhenAll(tasks);
                    });
                }
                else if (args.RestartServer || args.RestartServers)
                {
                    react = new Func<Task>(async () =>
                    {
                        var tasks = serverContexts.Select(x => Task.Run(async () =>
                        {
                            string message = null;
                            if (!await _arkServerService.RestartServer(x.Config.Key, (s) => { message = s; return Task.FromResult((IUserMessage)null); }))
                            {
                                Logging.Log($@"Countdown to restart server ({x.Config.Key}) execution failed (""{message ?? ""}"")", GetType(), LogLevel.DEBUG);
                            }
                        })).ToArray();

                        await Task.WhenAll(tasks);
                    });
                }

                sb.AppendLine($"**Countdown{(serverContext == null ? "" : $" on server {serverContext.Config.Key}")} have been initiated. Announcement will be made.**");
                await _scheduledTasksManager.StartCountdown(serverContext, reason, delayInMinutes, react);
            }
            else if (args.TerminateServer)
            {
                var dm = new DiscordMessage(Context.Channel, Context.User.Id);
                await _arkServerService.ShutdownServer(serverContext.Config.Key, (s) => dm.SendOrUpdateMessageDirectedAt($"{serverContext.Config.Key}: {s}"), true, true);
            }
            else if (args.StartServer || args.StartServers)
            {
                var tasks = serverContexts.Select(x => Task.Run(async () =>
                {
                    var dm = new DiscordMessage(Context.Channel, Context.User.Id);
                    await _arkServerService.StartServer(x.Config.Key, (s) => dm.SendOrUpdateMessageDirectedAt($"{x.Config.Key}: {s}"));
                })).ToArray();

                await Task.WhenAll(tasks);
            }
            else if (args.Countdown == null && (args.ShutdownServer || args.ShutdownServers || args.StopServer || args.StopServers))
            {
                var tasks = serverContexts.Select(x => Task.Run(async () =>
                {
                    var dm = new DiscordMessage(Context.Channel, Context.User.Id);
                    await _arkServerService.ShutdownServer(x.Config.Key, (s) => dm.SendOrUpdateMessageDirectedAt($"{x.Config.Key}: {s}"));
                })).ToArray();

                await Task.WhenAll(tasks);
            }
            else if (args.Countdown == null && (args.RestartServer || args.RestartServers))
            {
                var tasks = serverContexts.Select(x => Task.Run(async () =>
                {
                    var dm = new DiscordMessage(Context.Channel, Context.User.Id);
                    await _arkServerService.RestartServer(x.Config.Key, (s) => dm.SendOrUpdateMessageDirectedAt($"{x.Config.Key}: {s}"));
                })).ToArray();

                await Task.WhenAll(tasks);
            }
            else if (args.Countdown == null && (args.UpdateServer || args.UpdateServers))
            {
                var tasks = serverContexts.Select(x => Task.Run(async () =>
                {
                    var dm = new DiscordMessage(Context.Channel, Context.User.Id);
                    await _arkServerService.UpdateServer(x.Config.Key, (s) => dm.SendOrUpdateMessageDirectedAt($"{x.Config.Key}: {s}"), (s) => Context.Channel.GetMessageDirectedAtText(Context.User.Id, $"{x.Config.Key}: {s}"), 300);
                })).ToArray();

                await Task.WhenAll(tasks);
            }
            else if (args.SaveWorld)
            {
                await _arkServerService.SaveWorld(serverContext.Config.Key, (s) => Context.Channel.SendMessageDirectedAt(Context.User.Id, s), 180);
            }
            else if (args.Backups)
            {
                var result = _savegameBackupService.GetBackupsList(new[] { serverContext.Config.Key });
                if (result?.Count > 0)
                {
                    var data = result.OrderByDescending(x => x.DateModified).Take(25).Select(x => new
                    {
                        Path = x.Path,
                        Age = (DateTime.Now - x.DateModified).ToStringCustom(),
                        FileSize = x.ByteSize.ToFileSize()
                    }).ToArray();
                    var table = FixedWidthTableHelper.ToString(data, x => x
                        .For(y => y.Path, header: "Backup")
                        .For(y => y.Age, alignment: 1)
                        .For(y => y.FileSize, header: "File Size", alignment: 1));
                    sb.Append($"```{table}```");
                }
                else sb.AppendLine("**Could not find any savegame backups...**");
            }
            else if (args.DestroyWildDinos)
            {
                var result = await serverContext.Steam.SendRconCommand("destroywilddinos");
                if (result == null) sb.AppendLine("**Failed to wipe wild dinos... :(**");
                else sb.AppendLine("**Wild dinos wiped!**");
            }
            else if (args.DoExit)
            {
                var result = await serverContext.Steam.SendRconCommand("doexit");
                if (result == null) sb.AppendLine("**Failed to shutdown server... :(**");
                else sb.AppendLine("**Server shutting down!**");
            }
            else if (args.ListPlayers)
            {
                var result = await serverContext.Steam.SendRconCommand("listplayers");
                if (result == null) sb.AppendLine("**Failed to get a list of players... :(**");
                else sb.AppendLine(result);
            }
            else if (!string.IsNullOrWhiteSpace(args.Broadcast))
            {
                var result = await serverContext.Steam.SendRconCommand($"broadcast {args.Broadcast}");
                if (result == null) sb.AppendLine("**Failed to broadcast message... :(**");
                else sb.AppendLine("**Broadcast successfull!**");
            }
            else if (!string.IsNullOrWhiteSpace(args.ServerChat))
            {
                var result = await serverContext.Steam.SendRconCommand($"serverchat {args.ServerChat}");
                if (result == null) sb.AppendLine("**Failed to send chat message... :(**");
                else sb.AppendLine("**Chat message sent!**");
            }
            else if (args.ServerChatTo > 0 && !string.IsNullOrWhiteSpace(args.Message))
            {
                var result = await serverContext.Steam.SendRconCommand($@"serverchatto {args.ServerChatTo} ""{args.Message}""");
                if (result == null) sb.AppendLine("**Failed to send direct chat message... :(**");
                else sb.AppendLine("**Direct chat message sent!**");
            }
            else if (!string.IsNullOrWhiteSpace(args.ServerChatToPlayer) && !string.IsNullOrWhiteSpace(args.Message))
            {
                var result = await serverContext.Steam.SendRconCommand($@"serverchattoplayer ""{args.ServerChatToPlayer}"" ""{args.Message}""");
                if (result == null) sb.AppendLine("**Failed to send direct chat message... :(**");
                else sb.AppendLine("**Direct chat message sent!**");
            }
            else if (!string.IsNullOrWhiteSpace(args.RenamePlayer) && !string.IsNullOrWhiteSpace(args.NewName))
            {
                var result = await serverContext.Steam.SendRconCommand($@"renameplayer ""{args.RenamePlayer}"" {args.NewName}");
                if (result == null) sb.AppendLine("**Failed to rename player... :(**");
                else sb.AppendLine("**Player renamed!**");
            }
            else if (!string.IsNullOrWhiteSpace(args.RenameTribe) && !string.IsNullOrWhiteSpace(args.NewName))
            {
                var result = await serverContext.Steam.SendRconCommand($@"renametribe ""{args.RenameTribe}"" {args.NewName}");
                if (result == null) sb.AppendLine("**Failed to rename tribe... :(**");
                else sb.AppendLine("**Tribe renamed!**");
            }
            else if (!string.IsNullOrWhiteSpace(args.SetTimeOfDay) && _rTimeOfDay.IsMatch(args.SetTimeOfDay))
            {
                var result = await serverContext.Steam.SendRconCommand($"settimeofday {args.SetTimeOfDay}");
                if (result == null) sb.AppendLine("**Failed to set time of day... :(**");
                else sb.AppendLine("**Time of day set!**");
            }
            else if (args.KickPlayer > 0)
            {
                var result = await serverContext.Steam.SendRconCommand($"kickplayer {args.KickPlayer}");
                if (result == null) sb.AppendLine($"**Failed to kick player with steamid {args.KickPlayer}... :(**");
                else sb.AppendLine($"**Kicked player with steamid {args.KickPlayer}!**");
            }
            //else if (args.EnableVoting)
            //{
            //    if (!(args.True || args.False))
            //    {
            //        sb.AppendLine($"**This command requires additional arguments!**");
            //    }
            //    else
            //    {
            //        using (var context = _databaseContextFactory.Create())
            //        {
            //            _savedstate.VotingDisabled = !args.True;
            //            _savedstate.Save();
            //            sb.AppendLine($"**Voting system is now {(_savedstate.VotingDisabled ? "disabled" : "enabled")}!**");
            //        }
            //    }

            //    //var result = await CommandHelper.SendRconCommand(_config, $"kickplayer {args.KickPlayer}");
            //    //if (result == null) sb.AppendLine($"**Failed to kick player with steamid {args.KickPlayer}... :(**");
            //    //else sb.AppendLine($"**Kicked player with steamid {args.KickPlayer}!**");
            //}
            //else if (args.SetVotingAllowed > 0)
            //{
            //    if (!(args.True || args.False))
            //    {
            //        sb.AppendLine($"**This command requires additional arguments!**");
            //    }
            //    else
            //    {
            //        using (var context = _databaseContextFactory.Create())
            //        {
            //            var user = context.Users.FirstOrDefault(x => x != null && x.DiscordId == (long)Context.User.Id);
            //            if (user != null)
            //            {
            //                user.DisallowVoting = !args.True;
            //                context.SaveChanges();
            //                sb.AppendLine($"**The user is now {(user.DisallowVoting ? "unable" : "allowed")} to vote!**");
            //            }
            //            else
            //            {
            //                sb.AppendLine($"**The user is not linked!**");
            //            }
            //        }
            //    }

            //    //var result = await CommandHelper.SendRconCommand(_config, $"kickplayer {args.KickPlayer}");
            //    //if (result == null) sb.AppendLine($"**Failed to kick player with steamid {args.KickPlayer}... :(**");
            //    //else sb.AppendLine($"**Kicked player with steamid {args.KickPlayer}!**");
            //}
            else if (args.BanPlayer > 0)
            {
                var result = await serverContext.Steam.SendRconCommand($"ban {args.BanPlayer}");
                if (result == null) sb.AppendLine($"**Failed to ban player with steamid {args.BanPlayer}... :(**");
                else sb.AppendLine($"**Banned player with steamid {args.BanPlayer}!**");
            }
            else if (args.UnbanPlayer > 0)
            {
                var result = await serverContext.Steam.SendRconCommand($"unban {args.UnbanPlayer}");
                if (result == null) sb.AppendLine($"**Failed to unban player with steamid {args.UnbanPlayer}... :(**");
                else sb.AppendLine($"**Unbanned player with steamid {args.UnbanPlayer}!**");
            }
            else if (args.KillPlayer > 0)
            {
                var result = await serverContext.Steam.SendRconCommand($"killplayer {args.KillPlayer}");
                if (result == null) sb.AppendLine($"**Failed to kill player with id {args.KillPlayer}... :(**");
                else sb.AppendLine($"**Killed player with id {args.KillPlayer}!**");
            }
            else if (args.GetSteamIDForPlayerID > 0)
            {
                var result = await serverContext.Steam.SendRconCommand($"GetSteamIDForPlayerID {args.GetSteamIDForPlayerID}");
                if (result == null) sb.AppendLine($"**Failed to get steamid from id {args.GetSteamIDForPlayerID}... :(**");
                else sb.AppendLine(result);
            }
            else if (args.GetPlayerIDForSteamID > 0)
            {
                var result = await serverContext.Steam.SendRconCommand($"GetPlayerIDForSteamID {args.GetPlayerIDForSteamID}");
                if (result == null) sb.AppendLine($"**Failed to get id from steamid {args.GetPlayerIDForSteamID}... :(**");
                else sb.AppendLine(result);
            }
            else if (args.GetTribeIdPlayerList > 0)
            {
                var result = await serverContext.Steam.SendRconCommand($"GetTribeIdPlayerList {args.GetTribeIdPlayerList}");
                if (result == null) sb.AppendLine("**Failed to get a list of players in tribe... :(**");
                else sb.AppendLine(result);
            }
            else
            {
                var syntaxHelp = MethodBase.GetCurrentMethod().GetCustomAttribute<SyntaxHelpAttribute>()?.SyntaxHelp;
                var name = MethodBase.GetCurrentMethod().GetCustomAttribute<CommandAttribute>()?.Text;

                await Context.Channel.SendMessageAsync(string.Join(Environment.NewLine, new string[] {
                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
                    !string.IsNullOrWhiteSpace(syntaxHelp) ? $"Help me by following this syntax: **!{name}** {syntaxHelp}" : null }.Where(x => x != null)));
                return;
            }

            var msg = sb.ToString();
            if (!string.IsNullOrWhiteSpace(msg)) await CommandHelper.SendPartitioned(Context.Channel, sb.ToString());
        }
    }
}