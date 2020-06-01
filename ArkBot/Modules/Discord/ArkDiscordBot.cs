//TODO [.NET Core]: Removed temporarily
//using ArkBot.OpenID;
using ArkBot.Modules.Application;
using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.Application.Data;
using ArkBot.Modules.Database;
using ArkBot.Modules.Discord.Attributes;
using ArkBot.Modules.Shared;
using ArkBot.Utils;
using Autofac;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Modules.Discord
{
    public class ArkDiscordBot : IDisposable
    {
        private DiscordSocketClient _discord;
        private CommandService _commands;
        private IServiceProvider _serviceProvider;
        private IConfig _config;
        private IConstants _constants;
        private EfDatabaseContextFactory _databaseContextFactory;
        private ILifetimeScope _scope;
        private ArkContextManager _contextManager;

        private bool _wasRestarted;
        private List<ulong> _wasRestartedServersNotified = new List<ulong>();

        public ArkDiscordBot(
            DiscordSocketClient discord,
            CommandService commands,
            IServiceProvider serviceProvider,
            IConfig config,
            IConstants constants,
            EfDatabaseContextFactory databaseContextFactory,
            ILifetimeScope scope,
            ArkContextManager contextManager)
        {
            _discord = discord;
            _commands = commands;
            _serviceProvider = serviceProvider;
            _config = config;
            _constants = constants;
            _databaseContextFactory = databaseContextFactory;
            _scope = scope;
            _contextManager = contextManager;

            _discord.GuildAvailable += DiscordOnGuildAvailable;
            _discord.MessageReceived += HandleCommandAsync;

            var args = Environment.GetCommandLineArgs();
            if (args != null && args.Contains("/restart", StringComparer.OrdinalIgnoreCase))
            {
                _wasRestarted = true;
            }
        }

        private async Task DiscordOnGuildAvailable(SocketGuild socketGuild)
        {
            if (_wasRestarted && socketGuild != null && !string.IsNullOrWhiteSpace(_config.Discord.AnnouncementChannel) && !_wasRestartedServersNotified.Contains(socketGuild.Id))
            {
                try
                {
                    _wasRestartedServersNotified.Add(socketGuild.Id);
                    var channel = socketGuild.TextChannels.FirstOrDefault(y => _config.Discord.AnnouncementChannel.Equals(y.Name, StringComparison.OrdinalIgnoreCase));
                    if (channel != null) await channel.SendMessageAsync("**I have automatically restarted due to previous unexpected shutdown!**");
                }
                catch (Exception ex) { /*ignore exceptions */ }
            }
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            try
            {
                var message = messageParam as SocketUserMessage;
                if (message == null) return;

                var argPos = 0;
                if (!(message.HasCharPrefix('!', ref argPos) ||
                      message.HasMentionPrefix(_discord.CurrentUser, ref argPos)))
                    return;

                var context = new SocketCommandContext(_discord, message);

                var result = _commands.Search(context, argPos);
                if (result.IsSuccess && result.Commands.Count > 0)
                {
                    if (result.Commands.Count > 1)
                    {
                        Logging.Log(
                            $"Multiple commands registered for '{message.Content.Substring(argPos)}'! Skipping!",
                            GetType(), LogLevel.WARN);
                        return;
                    }

                    var cm = result.Commands.First();
                    var iCommand = cm.Command;
                    var iModule = iCommand.Module;
                    var isHidden = CommandHiddenAttribute.IsHidden(iModule.Attributes, iCommand.Attributes);

                    //check if command is allowed in this channel
                    if (!(context.Channel is ISocketPrivateChannel)
                        && _config.Discord.EnabledChannels?.Count > 0
                        && !_config.Discord.EnabledChannels.Contains(context.Channel.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    var preconditions = await cm.CheckPreconditionsAsync(context, _serviceProvider);
                    if (!preconditions.IsSuccess)
                    {
                        if (preconditions.ErrorReason?.Equals(RoleRestrictedPreconditionAttribute.CommandDisabledErrorString) == true) return;

                        Logging.Log(
                            $"Command precondition(s) failed [command name: {iCommand.Name}, preconditions error: {preconditions.ErrorReason}]",
                            GetType(), LogLevel.DEBUG);

                        await messageParam.Channel.SendMessageAsync("**The specified command is not available to your role or may not be used in this channel.**");

                        return;
                    }

                    var parseResult = await cm.ParseAsync(context, result, preconditions, _serviceProvider);
                    if (!parseResult.IsSuccess)
                    {
                        Logging.Log(
                            $"Command parsing failed [command name: {iCommand.Name}, parseResult error: {parseResult.ErrorReason}]",
                            GetType(), LogLevel.DEBUG);

                        var syntaxHelp = iCommand.Attributes.OfType<SyntaxHelpAttribute>().FirstOrDefault()?.SyntaxHelp;
                        var name = iCommand.Attributes.OfType<CommandAttribute>().FirstOrDefault()?.Text;

                        await messageParam.Channel.SendMessageAsync(string.Join(Environment.NewLine, new string[] {
                            $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
                            !string.IsNullOrWhiteSpace(syntaxHelp) ? $"Help me by following this syntax: **!{name}** {syntaxHelp}" : null }.Where(x => x != null)));

                        return;
                    }

                    var commandResult = await cm.ExecuteAsync(context, parseResult, _serviceProvider);
                    if (commandResult.IsSuccess)
                    {
                        if (isHidden) return;

                        var sb = new StringBuilder();
                        sb.AppendLine($@"""!{message.Content.Substring(argPos)}"" command successful!");
                        Logging.Log(sb.ToString(), GetType(), LogLevel.INFO);
                    }
                    else
                    {
                        if (isHidden || preconditions.Error.HasValue &&
                                         preconditions.Error.Value == CommandError.UnmetPrecondition)
                        {
                            if (!isHidden)
                                Logging.Log(
                                    $"Command unmet precondition(s) [command name: {iCommand.Name}, preconditions error: {preconditions.ErrorReason}]",
                                    GetType(), LogLevel.DEBUG);
                            return;
                        }

                        //if there is an exception log all information pertaining to it so that we can possibly fix it in the future
                        var exception = commandResult is ExecuteResult
                            ? ((ExecuteResult)commandResult).Exception
                            : null;
                        if (exception != null)
                        {
                            var errorMessage = $@"""!{message.Content.Substring(argPos)}"" command error...";

                            Logging.LogException(errorMessage, exception, GetType(), LogLevel.ERROR,
                                ExceptionLevel.Unhandled);
                        }

                        Logging.Log(
                            $"Command execution failed [command name: {iCommand.Name}, command error: {commandResult.ErrorReason}]",
                            GetType(), LogLevel.DEBUG);
                    }
                }

                //var result = await _commands.ExecuteAsync(context, argPos, _serviceProvider);
            }
            catch (Exception ex)
            {
                Logging.Log(
                    $"Command handler unhandled exception [message: {ex.Message}]",
                    GetType(), LogLevel.DEBUG);
            }
        }

        public async Task Initialize(CancellationToken token, bool skipExtract = false)
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        public async Task Start(ArkSpeciesAliases aliases = null)
        {
            await _discord.LoginAsync(TokenType.Bot, _config.Discord.BotToken);
            await _discord.StartAsync();
        }

        public async Task Stop()
        {
            await _discord.StopAsync();
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

                    //TODO: [.NET Core]: Removed temporarily
                    //_openId.SteamOpenIdCallback -= _openId_SteamOpenIdCallback; 
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
