extern alias DotNetZip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Helpers;
using ArkBot.Extensions;
using static System.FormattableString;
using System.Drawing;
using System.Text.RegularExpressions;
using QueryMaster.GameServer;
using System.Runtime.Caching;
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.IO.Compression;
using Autofac;
using ArkBot.Database;
using System.Diagnostics;
using Discord;
using ArkBot.Services;

namespace ArkBot.Commands.Experimental
{
    public class AdminCommand : IRoleRestrictedCommand, IEnabledCheckCommand
    {
        public string Name => "admin";
        public string[] Aliases => null;
        public string Description => "Admin commands to manage the ARK Server (rcon etc.)";
        public string SyntaxHelp => null;
        public string[] UsageExamples => new[]
        {
            "**SaveWorld**: Forces the server to save the game world to disk in its current state",
            "**DestroyWildDinos**: Destroys all untamed creatures on the map (which includes all creatures that are currently being tamed)",
            "**KickPlayer <steamid>**: Kick a player",
            "**BanPlayer <steamid>**: Ban a player",
            "**UnbanPlayer <steamid>**: Unban a player",
            "**KillPlayer <player id>**: Kill a player",
            "**SetVotingAllowed <steamid> true/false**: Set voting allowed/disallowed for a player",
            "**EnableVoting true/false**: Enable voting system",
            "**DoExit**: Shutdown server",
            "**Broadcast <message>**: Broadcast a message to all players on the server",
            "**ListPlayers**: List all connected players and their SteamIDs",
            "**RenamePlayer <old name> NewName <new name>**: Rename a player",
            "**RenameTribe <old name> NewName <new name>**: Rename a tribe",
            "**ServerChat <message>**: Sends a chat message to all currently connected players",
            "**ServerChatTo <steamid> Message <message>**: Sends a direct chat message to the player specified by their int64 encoded steam id",
            "**ServerChatToPlayer <name> Message <message>**: Sends a direct chat message to the player specified by their in-game player name",
            "**GetPlayerIDForSteamID <steamid>**: Get player id from given steamid",
            "**GetSteamIDForPlayerID <player id>**: Get steamid from give player id",
            "**GetTribeIdPlayerList <tribe id>**: Get a list of players in a tribe",
            "**SetTimeOfDay hh:mm:ss**: Sets the game world's time of day to the specified time",
            "**restartserver**: Restart the server.",
            "**updateserver**: Update the server.",
            "**startserver**: Start the server.",
            "**stopserver**: Stop/shutdown the server."
        };

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        public string[] ForRoles => new[] { _config.AdminRoleName, _config.DeveloperRoleName };

        public bool EnabledCheck()
        {
            return !string.IsNullOrWhiteSpace(_config.RconPassword) && _config.RconPort > 0;
        }

        private IArkContext _context;
        private IConfig _config;
        private IConstants _constants;
        private EfDatabaseContextFactory _databaseContextFactory;
        private ISavedState _savedstate;
        private IArkServerService _arkServerService;
        private ISavegameBackupService _savegameBackupService;

        public AdminCommand(
            ILifetimeScope scope, 
            IArkContext context, 
            IConfig config, 
            IConstants constants, 
            EfDatabaseContextFactory databaseContextFactory, 
            ISavedState savedstate, 
            IArkServerService arkServerService,
            ISavegameBackupService savegameBackupService)
        {
            _context = context;
            _config = config;
            _constants = constants;
            _databaseContextFactory = databaseContextFactory;
            _savedstate = savedstate;
            _arkServerService = arkServerService;
            _savegameBackupService = savegameBackupService;
        }

        public void Register(CommandBuilder command)
        {
            command.Parameter("optional", ParameterType.Multiple);
        }

        public void Init(Discord.DiscordClient client) { }

        public async Task Run(CommandEventArgs e)
        {
            if (!e.Channel.IsPrivate) return;

            var args = CommandHelper.ParseArgs(e, new
            {
                StartServer = false,
                ShutdownServer = false,
                StopServer = false,
                RestartServer = false,
                UpdateServer = false,
                Backups = false,
                SaveWorld = false,
                DestroyWildDinos = false,
                EnableVoting = false,
                SetVotingAllowed = 0L, //steam id
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
                True = false,
                False = false
            }, x =>
                x.For(y => y.StartServer, flag: true)
                .For(y => y.ShutdownServer, flag: true)
                .For(y => y.StopServer, flag: true)
                .For(y => y.RestartServer, flag: true)
                .For(y => y.UpdateServer, flag: true)
                .For(y => y.Backups, flag: true)
                .For(y => y.SaveWorld, flag: true)
                .For(y => y.DestroyWildDinos, flag: true)
                .For(y => y.EnableVoting, flag: true)
                .For(y => y.DoExit, flag: true)
                .For(y => y.ListPlayers, flag: true)
                .For(y => y.Broadcast, untilNextToken: true)
                .For(y => y.RenamePlayer, untilNextToken: true)
                .For(y => y.RenameTribe, untilNextToken: true)
                .For(y => y.NewName, untilNextToken: true)
                .For(y => y.ServerChat, untilNextToken: true)
                .For(y => y.ServerChatToPlayer, untilNextToken: true)
                .For(y => y.Message, untilNextToken: true)
                .For(y => y.True, flag: true)
                .For(y => y.False, flag: true));

            var _rTimeOfDay = new Regex(@"^\s*\d{2,2}\:\d{2,2}(\:\d{2,2})?\s*$", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            var sb = new StringBuilder();

            if (args.StartServer)
            {
                await _arkServerService.StartServer((s) => e.Channel.SendMessageDirectedAt(e.User.Id, s));
            }
            else if (args.ShutdownServer || args.StopServer)
            {
                await _arkServerService.ShutdownServer((s) => e.Channel.SendMessageDirectedAt(e.User.Id, s));
            }
            else if (args.RestartServer)
            {
                await _arkServerService.RestartServer((s) => e.Channel.SendMessageDirectedAt(e.User.Id, s));
            }
            else if (args.UpdateServer)
            {
                await _arkServerService.UpdateServer((s) => e.Channel.SendMessageDirectedAt(e.User.Id, s), (s) => e.Channel.GetMessageDirectedAtText(e.User.Id, s));
            }
            else if (args.SaveWorld)
            {
                await _arkServerService.SaveWorld((s) => e.Channel.SendMessageDirectedAt(e.User.Id, s), 180);
            }
            else if (args.Backups)
            {
                var result = _savegameBackupService.GetBackupsList();
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
                var result = await CommandHelper.SendRconCommand(_config, "destroywilddinos");
                if (result == null) sb.AppendLine("**Failed to wipe wild dinos... :(**");
                else sb.AppendLine("**Wild dinos wiped!**");
            }
            else if (args.DoExit)
            {
                var result = await CommandHelper.SendRconCommand(_config, "doexit");
                if (result == null) sb.AppendLine("**Failed to shutdown server... :(**");
                else sb.AppendLine("**Server shutting down!**");
            }
            else if (args.ListPlayers)
            {
                var result = await CommandHelper.SendRconCommand(_config, "listplayers");
                if (result == null) sb.AppendLine("**Failed to get a list of players... :(**");
                else sb.AppendLine(result);
            }
            else if (!string.IsNullOrWhiteSpace(args.Broadcast))
            {
                var result = await CommandHelper.SendRconCommand(_config, $"broadcast {args.Broadcast}");
                if (result == null) sb.AppendLine("**Failed to broadcast message... :(**");
                else sb.AppendLine("**Broadcast successfull!**");
            }
            else if (!string.IsNullOrWhiteSpace(args.ServerChat))
            {
                var result = await CommandHelper.SendRconCommand(_config, $"serverchat {args.ServerChat}");
                if (result == null) sb.AppendLine("**Failed to send chat message... :(**");
                else sb.AppendLine("**Chat message send!**");
            }
            else if (args.ServerChatTo > 0 && !string.IsNullOrWhiteSpace(args.Message))
            {
                var result = await CommandHelper.SendRconCommand(_config, $@"serverchatto {args.ServerChatTo} ""{args.Message}""");
                if (result == null) sb.AppendLine("**Failed to send direct chat message... :(**");
                else sb.AppendLine("**Direct chat message send!**");
            }
            else if (!string.IsNullOrWhiteSpace(args.ServerChatToPlayer) && !string.IsNullOrWhiteSpace(args.Message))
            {
                var result = await CommandHelper.SendRconCommand(_config, $@"serverchattoplayer ""{args.ServerChatToPlayer}"" ""{args.Message}""");
                if (result == null) sb.AppendLine("**Failed to send direct chat message... :(**");
                else sb.AppendLine("**Direct chat message send!**");
            }
            else if (!string.IsNullOrWhiteSpace(args.RenamePlayer) && !string.IsNullOrWhiteSpace(args.NewName))
            {
                var result = await CommandHelper.SendRconCommand(_config, $@"renameplayer ""{args.RenamePlayer}"" {args.NewName}");
                if (result == null) sb.AppendLine("**Failed to rename player... :(**");
                else sb.AppendLine("**Player renamed!**");
            }
            else if (!string.IsNullOrWhiteSpace(args.RenameTribe) && !string.IsNullOrWhiteSpace(args.NewName))
            {
                var result = await CommandHelper.SendRconCommand(_config, $@"renametribe ""{args.RenameTribe}"" {args.NewName}");
                if (result == null) sb.AppendLine("**Failed to rename tribe... :(**");
                else sb.AppendLine("**Tribe renamed!**");
            }
            else if (!string.IsNullOrWhiteSpace(args.SetTimeOfDay) && _rTimeOfDay.IsMatch(args.SetTimeOfDay))
            {
                var result = await CommandHelper.SendRconCommand(_config, $"settimeofday {args.SetTimeOfDay}");
                if (result == null) sb.AppendLine("**Failed to set time of day... :(**");
                else sb.AppendLine("**Time of day set!**");
            }
            else if (args.KickPlayer > 0)
            {
                var result = await CommandHelper.SendRconCommand(_config, $"kickplayer {args.KickPlayer}");
                if (result == null) sb.AppendLine($"**Failed to kick player with steamid {args.KickPlayer}... :(**");
                else sb.AppendLine($"**Kicked player with steamid {args.KickPlayer}!**");
            }
            else if (args.EnableVoting)
            {
                if (!(args.True || args.False))
                {
                    sb.AppendLine($"**This command requires additional arguments!**");
                }
                else
                {
                    using (var context = _databaseContextFactory.Create())
                    {
                        _savedstate.VotingDisabled = !args.True;
                        _savedstate.Save();
                        sb.AppendLine($"**Voting system is now {(_savedstate.VotingDisabled ? "disabled" : "enabled")}!**");
                    }
                }

                //var result = await CommandHelper.SendRconCommand(_config, $"kickplayer {args.KickPlayer}");
                //if (result == null) sb.AppendLine($"**Failed to kick player with steamid {args.KickPlayer}... :(**");
                //else sb.AppendLine($"**Kicked player with steamid {args.KickPlayer}!**");
            }
            else if (args.SetVotingAllowed > 0)
            {
                if(!(args.True || args.False))
                {
                    sb.AppendLine($"**This command requires additional arguments!**");
                }
                else
                {
                    using (var context = _databaseContextFactory.Create())
                    {
                        var user = context.Users.FirstOrDefault(x => x != null && x.DiscordId == (long)e.User.Id);
                        if (user != null)
                        {
                            user.DisallowVoting = !args.True;
                            context.SaveChanges();
                            sb.AppendLine($"**The user is now {(user.DisallowVoting ? "unable" : "allowed")} to vote!**");
                        }
                        else
                        {
                            sb.AppendLine($"**The user is not linked!**");
                        }
                    }
                }

                //var result = await CommandHelper.SendRconCommand(_config, $"kickplayer {args.KickPlayer}");
                //if (result == null) sb.AppendLine($"**Failed to kick player with steamid {args.KickPlayer}... :(**");
                //else sb.AppendLine($"**Kicked player with steamid {args.KickPlayer}!**");
            }
            else if (args.BanPlayer > 0)
            {
                var result = await CommandHelper.SendRconCommand(_config, $"ban {args.BanPlayer}");
                if (result == null) sb.AppendLine($"**Failed to ban player with steamid {args.BanPlayer}... :(**");
                else sb.AppendLine($"**Banned player with steamid {args.BanPlayer}!**");
            }
            else if (args.UnbanPlayer > 0)
            {
                var result = await CommandHelper.SendRconCommand(_config, $"unban {args.UnbanPlayer}");
                if (result == null) sb.AppendLine($"**Failed to unban player with steamid {args.UnbanPlayer}... :(**");
                else sb.AppendLine($"**Unbanned player with steamid {args.UnbanPlayer}!**");
            }
            else if (args.KillPlayer > 0)
            {
                var result = await CommandHelper.SendRconCommand(_config, $"killplayer {args.KillPlayer}");
                if (result == null) sb.AppendLine($"**Failed to kill player with id {args.KillPlayer}... :(**");
                else sb.AppendLine($"**Killed player with id {args.KillPlayer}!**");
            }
            else if (args.GetSteamIDForPlayerID > 0)
            {
                var result = await CommandHelper.SendRconCommand(_config, $"GetSteamIDForPlayerID {args.GetSteamIDForPlayerID}");
                if (result == null) sb.AppendLine($"**Failed to get steamid from id {args.GetSteamIDForPlayerID}... :(**");
                else sb.AppendLine(result);
            }
            else if (args.GetPlayerIDForSteamID > 0)
            {
                var result = await CommandHelper.SendRconCommand(_config, $"GetPlayerIDForSteamID {args.GetPlayerIDForSteamID}");
                if (result == null) sb.AppendLine($"**Failed to get id from steamid {args.GetPlayerIDForSteamID}... :(**");
                else sb.AppendLine(result);
            }
            else if (args.GetTribeIdPlayerList > 0)
            {
                var result = await CommandHelper.SendRconCommand(_config, $"GetTribeIdPlayerList {args.GetTribeIdPlayerList}");
                if (result == null) sb.AppendLine("**Failed to get a list of players in tribe... :(**");
                else sb.AppendLine(result);
            }
            else
            {
                await e.Channel.SendMessage(string.Join(Environment.NewLine, new string[] {
                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
                    !string.IsNullOrWhiteSpace(SyntaxHelp) ? $"Help me by following this syntax: **!{Name}** {SyntaxHelp}" : null }.Where(x => x != null)));
                return;
            }

            var msg = sb.ToString();
            if (!string.IsNullOrWhiteSpace(msg)) await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
        }
    }
}