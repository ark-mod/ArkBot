﻿using ArkBot.Modules.Application;
using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.Application.Data;
using ArkBot.Modules.Application.Services;
using ArkBot.Modules.Database;
using ArkBot.Modules.Discord.Attributes;
using ArkBot.Modules.Shared;
using ArkBot.Utils.Helpers;
using Autofac;
using Discord.Commands;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Modules.Discord.Commands.Admin
{
    public class RconCommand : ModuleBase<SocketCommandContext>
    {
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

        [CommandHidden]
        [Command("rcon")]
        [Summary("Rcon server administration")]
        [SyntaxHelp(null)]
        [UsageExamples(new[]
        {
            "**<server key> '<command>'**: Sends a custom rcon command to the server instance",
        })]
        [RoleRestrictedPrecondition("rcon")]
        public async Task Rcon([Remainder] string arguments = null)
        {
            var args = CommandHelper.ParseArgs(arguments, new
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
                await Context.Channel.SendMessageAsync($"**Rcon commands need to be prefixed with a valid server instance key.**");
                return;
            }

            if (args == null)
            {
                var syntaxHelp = MethodBase.GetCurrentMethod().GetCustomAttribute<SyntaxHelpAttribute>()?.SyntaxHelp;
                var name = MethodBase.GetCurrentMethod().GetCustomAttribute<CommandAttribute>()?.Text;

                await Context.Channel.SendMessageAsync(string.Join(Environment.NewLine, new string[] {
                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
                    !string.IsNullOrWhiteSpace(syntaxHelp) ? $"Help me by following this syntax: **!{name}** {syntaxHelp}" : null }.Where(x => x != null)));
                return;
            }

            var result = await serverContext.Steam.SendRconCommand(args.Command);
            if (result == null) sb.AppendLine("**Failed to send rcon command... :(**");
            else sb.AppendLine($"```{result}```");

            var msg = sb.ToString();
            if (!string.IsNullOrWhiteSpace(msg)) await CommandHelper.SendPartitioned(Context.Channel, sb.ToString());
        }
    }
}