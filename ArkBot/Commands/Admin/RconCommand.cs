extern alias DotNetZip;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Helpers;
using Autofac;
using ArkBot.Database;
using Discord;
using ArkBot.Services;
using ArkBot.Ark;
using ArkBot.ScheduledTasks;

namespace ArkBot.Commands.Admin
{
    public class RconCommand : IRoleRestrictedCommand
    {
        public string Name => "rcon";
        public string[] Aliases => null;
        public string Description => "Rcon server administration";
        public string SyntaxHelp => null;
        public string[] UsageExamples => new[]
        {
            "**<server key> '<command>'**: Sends a custom rcon command to the server instance",
        };

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        public string[] ForRoles => new[] { _config.AdminRoleName, _config.DeveloperRoleName };

        private IConfig _config;
        private ArkContextManager _contextManager;

        public RconCommand(
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
            _contextManager = contextManager;
        }

        public void Register(CommandBuilder command)
        {
            command.Parameter("optional", ParameterType.Multiple);
        }

        public void Init(DiscordClient client) { }

        public async Task Run(CommandEventArgs e)
        {
            var args = CommandHelper.ParseArgs(e, new
            {
                ServerKey = "",
                Command = ""
            }, x =>
                x.For(y => y.ServerKey, noPrefix: true, isRequired: true)
                .For(y => y.Command, noPrefix: true, isRequired: true, untilNextToken: true));

            var sb = new StringBuilder();

            var serverContext = args?.ServerKey != null ? _contextManager.GetServer(args.ServerKey) : null;
            if (serverContext == null)
            {
                await e.Channel.SendMessage($"**Rcon commands need to be prefixed with a valid server instance key.**");
                return;
            }

            if (args == null)
            {
                await e.Channel.SendMessage(string.Join(Environment.NewLine, new string[] {
                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
                    !string.IsNullOrWhiteSpace(SyntaxHelp) ? $"Help me by following this syntax: **!{Name}** {SyntaxHelp}" : null }.Where(x => x != null)));
                return;
            }

            
            var result = await serverContext.Steam.SendRconCommand(args.Command);
            if (result == null) sb.AppendLine("**Failed to send rcon command... :(**");
            else sb.AppendLine($"```{result}```");

            var msg = sb.ToString();
            if (!string.IsNullOrWhiteSpace(msg)) await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
        }
    }
}