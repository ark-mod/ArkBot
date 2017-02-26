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
        private DatabaseContextFactory<IEfDatabaseContext> _databaseContextFactory;

        public ArkDiscordBot(IConfig config, IArkContext context, IConstants constants, IBarebonesSteamOpenId openId, DatabaseContextFactory<IEfDatabaseContext> databaseContextFactory, IEnumerable<ICommand> commands)
        {
            _config = config;
            _context = context;
            _constants = constants;
            _databaseContextFactory = databaseContextFactory;
            _openId = openId;
            _openId.SteamOpenIdCallback += _openId_SteamOpenIdCallback;

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

            var cservice = _discord.GetService<CommandService>();
            cservice.CommandExecuted += Commands_CommandExecuted;
            cservice.CommandErrored += Commands_CommandErrored;
            foreach(var command in commands)
            {
                if (command.DebugOnly && !_config.Debug) continue;

                var cbuilder = cservice.CreateCommand(command.Name);
                if (command.Aliases != null && command.Aliases.Length > 0) cbuilder.Alias(command.Aliases);
                command.Register(cbuilder);
                cbuilder.Do(command.Run);
            }
        }

        private async void _openId_SteamOpenIdCallback(object sender, SteamOpenIdCallbackEventArgs e)
        {
            var ch = await _discord.CreatePrivateChannel(e.DiscordUserId);
            if (ch == null) return;

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
                        context.Users.Add(new Database.Model.User { DiscordId = (long)e.DiscordUserId, SteamId = (long)e.SteamId, RealName = player?.RealName, SteamDisplayName = player?.PersonaName });
                    }

                    context.SaveChanges();
                }
                await ch.SendMessage($"Your Discord user is now linked with your Steam account! :)");
            }
            else
            {
                await ch.SendMessage($"Something went wrong during the linking process. Please try again later!");
            }
        }

        private void Commands_CommandErrored(object sender, CommandErrorEventArgs e)
        {
            if (e == null || e.Command == null) return;
            var sb = new StringBuilder();
            sb.AppendLine($@"""!{e.Command.Text}{(e.Args.Length > 0 ? " " : "")}{string.Join(" ", e.Args)}"" command error...");
            if(e.Exception != null) sb.AppendLine($"Exception: {e.Exception.ToString()}");
            sb.AppendLine();
            _context.Progress.Report(sb.ToString());
        }

        private void Commands_CommandExecuted(object sender, CommandEventArgs e)
        {
            if (e == null || e.Command == null) return;

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
