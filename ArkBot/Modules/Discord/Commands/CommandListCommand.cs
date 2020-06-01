using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.Discord.Attributes;
using ArkBot.Utils.Helpers;
using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Modules.Discord.Commands
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

        [CommandHidden]
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
                var command = _commands.Commands.FirstOrDefault(x => name.Equals(x.Name, StringComparison.OrdinalIgnoreCase) || x.Aliases.Contains(name, StringComparer.OrdinalIgnoreCase));
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
}
