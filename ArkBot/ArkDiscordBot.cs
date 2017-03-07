using ArkBot.Commands;
using ArkBot.Data;
using ArkBot.Database;
using ArkBot.OpenID;
using Discord;
using Discord.Commands;
using Google.Apis.Urlshortener.v1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class ArkDiscordBot : IDisposable
    {
        private DiscordClient _discord;
        private IArkContext _context;
        private IConfig _config;
        private IConstants _constants;
        private IBarebonesSteamOpenId _openId;
        private EfDatabaseContextFactory _databaseContextFactory;

        public ArkDiscordBot(IConfig config, IArkContext context, IConstants constants, IBarebonesSteamOpenId openId, EfDatabaseContextFactory databaseContextFactory, IEnumerable<ICommand> commands)
        {
            _config = config;
            _context = context;
            _constants = constants;
            _databaseContextFactory = databaseContextFactory;
            _openId = openId;
            _openId.SteamOpenIdCallback += _openId_SteamOpenIdCallback;

            _context.Updated += _context_Updated;

            _discord = new DiscordClient(x =>
           {
               x.LogLevel = LogSeverity.Info;
               x.LogHandler += Log;
               x.AppName = _config.BotName;
               x.AppUrl = !string.IsNullOrWhiteSpace(_config.BotUrl) ? _config.BotUrl : null;
           });

            _discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            _discord.ServerAvailable += _discord_ServerAvailable;

            var cservice = _discord.GetService<CommandService>();
            cservice.CommandExecuted += Commands_CommandExecuted;
            cservice.CommandErrored += Commands_CommandErrored;
            foreach(var command in commands)
            {
                if (command.DebugOnly && !_config.Debug) continue;

                var cbuilder = cservice.CreateCommand(command.Name);
                if (command.Aliases != null && command.Aliases.Length > 0) cbuilder.Alias(command.Aliases);
                var rrc = command as IRoleRestrictedCommand;
                if (rrc != null && rrc.ForRoles?.Length > 0)
                {
                    cbuilder.AddCheck((a, b, c) => 
                    c.Client.Servers.Any(x => 
                    x.Roles.Any(y => y != null && rrc.ForRoles.Contains(y.Name, StringComparer.OrdinalIgnoreCase) == true && y.Members.Any(z => z.Id == b.Id))), null);
                }

                command.Init(_discord);
                command.Register(cbuilder);
                cbuilder.Do(command.Run);
            }
        }

        private async void _discord_ServerAvailable(object sender, ServerEventArgs e)
        {
            await UpdateNicknamesAndRoles(e.Server); 
        }

        /// <summary>
        /// All context data have been updated (occurs on start and when a savefile change have been handled)
        /// </summary>
        private async void _context_Updated(object sender, EventArgs e)
        {
            //on the first update triggered on start, servers are not yet connected so this code will not run.
            await UpdateNicknamesAndRoles();
        }

        private async Task UpdateNicknamesAndRoles(Server _server = null)
        {
            try
            {
                //change nicknames, add/remove from ark-role
                Database.Model.User[] linkedusers = null;
                using (var db = _databaseContextFactory.Create())
                {
                    linkedusers = db.Users.ToArray();
                }

                foreach (var server in _discord.Servers)
                {
                    if (_server != null && server.Id != _server.Id) continue;

                    var role = server.FindRoles("ark", true).FirstOrDefault();
                    if (role == null) continue;

                    foreach (var user in server.Users)
                    {
                        try
                        {
                            var dbuser = linkedusers.FirstOrDefault(x => (ulong)x.DiscordId == user.Id);
                            if (dbuser == null)
                            {
                                if (user.HasRole(role)) await user.RemoveRoles(role);
                                continue;
                            }

                            if (!user.HasRole(role)) await user.AddRoles(role);

                            var player = _context.Players?.FirstOrDefault(x => { long steamId = 0; return long.TryParse(x.SteamId, out steamId) ? steamId == dbuser.SteamId : false; });
                            var playerName = player?.Name?.Length > 32 ? player?.Name?.Substring(0, 32) : player?.Name;
                            if (playerName != null && !string.IsNullOrWhiteSpace(playerName) && (user.Nickname == null || !playerName.Equals(user.Nickname, StringComparison.Ordinal)))
                            {
                                //must be less or equal to 32 characters
                                await user.Edit(nickname: playerName);
                            }
                        }
                        catch (Discord.Net.HttpException)
                        {
                            //could be due to the order of roles on the server. bot role with "manage roles"/"change nickname" permission must be higher up than the role it is trying to set
                        }
                    }
                }
            }
            catch(WebException ex)
            {
                //do nothing
                ExceptionLogging.LogException(ex, $"Ignored {ex.GetType().Name} in {nameof(ArkDiscordBot)}.{nameof(UpdateNicknamesAndRoles)}");
            }
        }

        private async void _openId_SteamOpenIdCallback(object sender, SteamOpenIdCallbackEventArgs e)
        {
            if (e.Successful)
            {
                var player = new
                {
                    RealName = (string)null,
                    PersonaName = (string)null
                };
                try
                {
                    using (var wc = new WebClient())
                    {
                        var data = await wc.DownloadStringTaskAsync($@"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_config.SteamApiKey}&steamids={e.SteamId}");
                        var response = JsonConvert.DeserializeAnonymousType(data, new { response = new { players = new[] { player } } });
                        player = response?.response?.players?.FirstOrDefault();
                    }
                }
                catch { /* ignore exceptions */ }

                //QueryMaster.Steam.GetPlayerSummariesResponsePlayer player = null;
                //await Task.Factory.StartNew(() =>
                //{
                //    try
                //    {
                //        //this results in an exception (but it is easy enough to query by ourselves)
                //        var query = new QueryMaster.Steam.SteamQuery(_config.SteamApiKey);
                //        var result = query?.ISteamUser.GetPlayerSummaries(new[] { e.SteamId });
                //        if (result == null || !result.IsSuccess) return;

                //        player = result.ParsedResponse.Players.FirstOrDefault();
                //    }
                //    catch { /* ignore exceptions */}
                //});

                //set ark role on users when they link
                foreach(var server in _discord.Servers)
                {
                    var user = server.GetUser(e.DiscordUserId);
                    var role = server.FindRoles("ark", true).FirstOrDefault();
                    if (user == null || role == null) continue;

                    try
                    {
                        if (!user.HasRole(role)) await user.AddRoles(role);

                        var p = _context.Players?.FirstOrDefault(x => { ulong steamId = 0; return ulong.TryParse(x.SteamId, out steamId) ? steamId == e.SteamId : false; });
                        if (p != null && !string.IsNullOrWhiteSpace(p.Name))
                        {

                            //must be less or equal to 32 characters
                            await user.Edit(nickname: p.Name.Length > 32 ? p.Name.Substring(0, 32) : p.Name);

                        }
                    }
                    catch (Discord.Net.HttpException)
                    {
                        //could be due to the order of roles on the server. bot role with "manage roles"/"change nickname" permission must be higher up than the role it is trying to set
                    }
                }

                using (var context = _databaseContextFactory.Create())
                {
                    var user = context.Users.FirstOrDefault(x => x.DiscordId == (long)e.DiscordUserId);
                    if (user != null)
                    {
                        user.RealName = player?.RealName;
                        user.SteamDisplayName = player?.PersonaName;
                        user.SteamId = (long)e.SteamId;
                    }
                    else
                    {
                        user = new Database.Model.User { DiscordId = (long)e.DiscordUserId, SteamId = (long)e.SteamId, RealName = player?.RealName, SteamDisplayName = player?.PersonaName };
                        context.Users.Add(user);
                    }

                    foreach(var associatePlayed in context.Played.Where(x => x.SteamId == (long)e.SteamId))
                    {
                        associatePlayed.SteamId = null;
                        user.Played.Add(associatePlayed);
                    }

                    context.SaveChanges();
                }
                var ch = await _discord.CreatePrivateChannel(e.DiscordUserId);
                await ch?.SendMessage($"Your Discord user is now linked with your Steam account! :)");
            }
            else
            {
                var ch = await _discord.CreatePrivateChannel(e.DiscordUserId);
                await ch?.SendMessage($"Something went wrong during the linking process. Please try again later!");
            }
        }

        private void Commands_CommandErrored(object sender, CommandErrorEventArgs e)
        {
            if (e == null || e.Command == null || e.Command.IsHidden) return;
            var sb = new StringBuilder();
            var message = $@"""!{e.Command.Text}{(e.Args.Length > 0 ? " " : "")}{string.Join(" ", e.Args)}"" command error...";
            sb.AppendLine(message);
            if(e.Exception != null) sb.AppendLine($"Exception: {e.Exception.ToString()}");
            sb.AppendLine();
            _context.Progress.Report(sb.ToString());

            //log to disk
            ExceptionLogging.LogException(e.Exception, message: message, source: nameof(ArkDiscordBot) + "_command");
        }

        private void Commands_CommandExecuted(object sender, CommandEventArgs e)
        {
            if (e == null || e.Command == null || e.Command.IsHidden) return;

            var sb = new StringBuilder();
            sb.AppendLine($@"""!{e.Command.Text}{(e.Args.Length > 0 ? " " : "")}{string.Join(" ", e.Args)}"" command successful!");
            _context.Progress.Report(sb.ToString());
        }

        public async Task Start(ArkSpeciesAliases aliases = null)
        {
            await _context.Initialize(aliases);

            _context.Progress.Report("Initialization done, connecting bot..." + Environment.NewLine);
            await _discord.Connect(_config.BotToken, TokenType.Bot);
        }

        public async Task Stop()
        {
            await _discord.Disconnect();
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _discord?.Dispose();
                    _discord = null;

                    _openId.SteamOpenIdCallback -= _openId_SteamOpenIdCallback;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ArkBot() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
