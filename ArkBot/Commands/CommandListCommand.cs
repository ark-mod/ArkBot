using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Discord.Command;
using Discord.Commands;
using ArkBot.Helpers;
using Discord;
using Discord.Commands.Builders;
using Discord.WebSocket;
using RestSharp;
using ArkBot.Configuration.Model;

namespace ArkBot.Commands
{
    public class CommandListCommand : ModuleBase<SocketCommandContext>
    {
        private IConfig _config;
        private CommandService _commands;
        private IServiceProvider _serviceProvider;

        public CommandListCommand(
            IConfig config, 
            CommandService commands, 
            IServiceProvider serviceProvider)
        {
            _config = config;
            _commands = commands;
            _serviceProvider = serviceProvider;
        }

        [CommandHiddenAttribute]
        [Command("command")]
        [Alias("commands", "help")]
        [RoleRestrictedPrecondition("commands")]
        public async Task Commands(string _name = null)
        {
            var name = _name?.TrimStart('!').ToLower();
            var sb = new StringBuilder();
            if (string.IsNullOrWhiteSpace(name))
            {
                sb.AppendLine($"**List of commands** (for usage examples type **!commands** <***name of command***>");
                sb.AppendLine(@"***Tip:*** You can send commands to me in a private conversation if you wish.");
                sb.AppendLine();

                foreach (var command in _commands.Commands.OrderBy(x => x.Name))
                {
                    if (CommandHiddenAttribute.IsHidden(command.Module.Attributes, command.Attributes)) continue;

                    var context = new SocketCommandContext(Context.Client, Context.Message);

                    var precRoleRestricted = command.Preconditions.OfType<RoleRestrictedPreconditionAttribute>().FirstOrDefault();
                    if (precRoleRestricted != null && !(await precRoleRestricted.CheckPermissionsAsync(context, command, _serviceProvider)).IsSuccess) continue;

                    sb.AppendLine($"● **!{command.Name}**" + (!string.IsNullOrWhiteSpace(command.Summary) ? $": {command.Summary}" : ""));
                    var syntaxHelp = command.SyntaxHelp();
                    if (!string.IsNullOrWhiteSpace(syntaxHelp)) sb.AppendLine($"Syntax: **!{command.Name}** {syntaxHelp}");
                }
            }
            else
            {
                //var command = Commands.FirstOrDefault(x => !x.HideFromCommandList 
                //    && (!x.DebugOnly || _config.Debug)
                //    && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                var command = _commands.Commands.FirstOrDefault(x => /*!CommandHiddenAttribute.IsHidden(x.Module.Attributes, x.Attributes)
                    &&*/ name.Equals(x.Name, StringComparison.OrdinalIgnoreCase) || x.Aliases.Contains(name, StringComparer.OrdinalIgnoreCase));
                if (command == null)
                {
                    sb.AppendLine($"**The specified command does not exist!**");
                }
                else
                {
                    var context = new SocketCommandContext(Context.Client, Context.Message);

                    var usageExamples = command.UsageExamples();

                    var precRoleRestricted = command.Preconditions.OfType<RoleRestrictedPreconditionAttribute>().FirstOrDefault();
                    if (precRoleRestricted != null &&
                        !(await precRoleRestricted.CheckPermissionsAsync(context, command, _serviceProvider)).IsSuccess)
                    {
                        sb.AppendLine($"**The specified command is only available to some roles and using a private channel.**");
                    }
                    else if (usageExamples == null || usageExamples.Length <= 0)
                    {
                        sb.AppendLine($"**The specified command does not have any usage examples :(**");
                    }
                    else
                    {
                        sb.AppendLine($"**Example usage of !{name.ToLower()}**");
                        foreach (var usageExample in usageExamples)
                        {
                            if (string.IsNullOrWhiteSpace(usageExample)) continue;

                            sb.AppendLine($"● **!{command.Name}**" + (usageExample[0] == ':' ? "" : " ") + usageExample);
                        }
                    }
                }
            }

            await CommandHelper.SendPartitioned(Context.Channel, sb.ToString());
        }
    }

    //public class CommandListCommand : ICommand
    //{
    //    public string Name => "command";
    //    public string[] Aliases => new[] { "commands", "help" };
    //    public string Description => null;
    //    public string SyntaxHelp => null;
    //    public string[] UsageExamples => null;

    //    public bool DebugOnly => false;
    //    public bool HideFromCommandList => true;

    //    public IEnumerable<ICommand> Commands { get; set; }

    //    private IConfig _config;
    //    private DiscordClient _discord;

    //    public CommandListCommand(IConfig config)
    //    {
    //        _config = config;
    //    }

    //    public void Register(CommandBuilder command)
    //    {
    //        command.Parameter("name", ParameterType.Optional);
    //    }

    //    public void Init(DiscordClient client)
    //    {
    //        _discord = client;
    //    }

    //    public async Task Run(CommandEventArgs e)
    //    {
    //        var name = e.GetArg("name")?.TrimStart('!').ToLower();
    //        var sb = new StringBuilder();
    //        if (string.IsNullOrWhiteSpace(name))
    //        {
    //            sb.AppendLine($"**List of commands** (for usage examples type **!commands** <***name of command***>");
    //            //sb.AppendLine(@"***Note:*** Names containing spaces must be encosed in quotes (ex. ""Epic tribe"")!");
    //            //sb.AppendLine(@"***Note:*** Tribe owned dinos and structures are not owned by a specific player.");
    //            sb.AppendLine(@"***Tip:*** You can send commands to me in a private conversation if you wish.");
    //            sb.AppendLine();

    //            foreach (var command in Commands.OrderBy(x => x.Name))
    //            {
    //                //if (command.HideFromCommandList || (command.DebugOnly && !_config.Debug)) continue;
    //                if (command.HideFromCommandList) continue;

    //                var ecc = command as IEnabledCheckCommand;
    //                if (ecc != null && !ecc.EnabledCheck())
    //                    continue;

    //                var rrc = command as IRoleRestrictedCommand;
    //                if (rrc != null && (!e.Channel.IsPrivate || (rrc.ForRoles != null && rrc.ForRoles.Length > 0 
    //                    && !_discord.Servers.Any(x => x.Roles.Any(y => y != null && rrc.ForRoles.Contains(y.Name, StringComparer.OrdinalIgnoreCase) == true && y.Members.Any(z => z.Id == e.User.Id))))))
    //                    continue;

    //                sb.AppendLine($"● **!{command.Name}**" + (!string.IsNullOrWhiteSpace(command.Description) ? $": {command.Description}" : ""));
    //                if (!string.IsNullOrWhiteSpace(command.SyntaxHelp)) sb.AppendLine($"Syntax: **!{command.Name}** {command.SyntaxHelp}");
    //            }
    //        }
    //        else
    //        {
    //            //var command = Commands.FirstOrDefault(x => !x.HideFromCommandList 
    //            //    && (!x.DebugOnly || _config.Debug)
    //            //    && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    //            var command = Commands.FirstOrDefault(x => !x.HideFromCommandList
    //                && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    //            if (command == null)
    //            {
    //                sb.AppendLine($"**The specified command does not exist!**");
    //            }
    //            else
    //            {
    //                var rrc = command as IRoleRestrictedCommand;
    //                if (rrc != null && (!e.Channel.IsPrivate || (rrc.ForRoles != null && rrc.ForRoles.Length > 0
    //                    && !_discord.Servers.Any(x => x.Roles.Any(y => y != null && rrc.ForRoles.Contains(y.Name, StringComparer.OrdinalIgnoreCase) == true && y.Members.Any(z => z.Id == e.User.Id))))))
    //                {
    //                    sb.AppendLine($"**The specified command is only available to some roles and using a private channel.**");
    //                }
    //                else if (command.UsageExamples == null || command.UsageExamples.Length <= 0)
    //                {
    //                    sb.AppendLine($"**The specified command does not have any usage examples :(**");
    //                }
    //                else
    //                {
    //                    sb.AppendLine($"**Example usage of !{name.ToLower()}**");
    //                    foreach(var usageExample in command.UsageExamples)
    //                    {
    //                        if (string.IsNullOrWhiteSpace(usageExample)) continue;

    //                        sb.AppendLine($"● **!{command.Name}**" + (usageExample[0] == ':' ? "" : " ") + usageExample);
    //                    }
    //                }
    //            }
                
    //        }

    //        await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
    //    }
    //}
}
