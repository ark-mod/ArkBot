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
using Discord;

namespace ArkBot.Commands
{
    public class CommandListCommand : ICommand
    {
        public string Name => "command";
        public string[] Aliases => new[] { "commands", "help" };
        public string Description => null;
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => true;

        public IEnumerable<ICommand> Commands { get; set; }

        private IConfig _config;
        private DiscordClient _discord;

        public CommandListCommand(IConfig config)
        {
            _config = config;
        }

        public void Register(CommandBuilder command)
        {
            command.Parameter("name", ParameterType.Optional);
        }

        public void Init(DiscordClient client)
        {
            _discord = client;
        }

        public async Task Run(CommandEventArgs e)
        {
            var name = e.GetArg("name")?.TrimStart('!').ToLower();
            var sb = new StringBuilder();
            if (string.IsNullOrWhiteSpace(name))
            {
                sb.AppendLine($"**List of commands** (for usage examples type **!commands** <***name of command***>");
                //sb.AppendLine(@"***Note:*** Names containing spaces must be encosed in quotes (ex. ""Epic tribe"")!");
                //sb.AppendLine(@"***Note:*** Tribe owned dinos and structures are not owned by a specific player.");
                sb.AppendLine(@"***Tip:*** You can send commands to me in a private conversation if you wish.");
                sb.AppendLine();

                foreach (var command in Commands.OrderBy(x => x.Name))
                {
                    //if (command.HideFromCommandList || (command.DebugOnly && !_config.Debug)) continue;
                    if (command.HideFromCommandList) continue;

                    var ecc = command as IEnabledCheckCommand;
                    if (ecc != null && !ecc.EnabledCheck())
                        continue;

                    var rrc = command as IRoleRestrictedCommand;
                    if (rrc != null && (!e.Channel.IsPrivate || (rrc.ForRoles != null && rrc.ForRoles.Length > 0 
                        && !_discord.Servers.Any(x => x.Roles.Any(y => y != null && rrc.ForRoles.Contains(y.Name, StringComparer.OrdinalIgnoreCase) == true && y.Members.Any(z => z.Id == e.User.Id))))))
                        continue;

                    sb.AppendLine($"● **!{command.Name}**" + (!string.IsNullOrWhiteSpace(command.Description) ? $": {command.Description}" : ""));
                    if (!string.IsNullOrWhiteSpace(command.SyntaxHelp)) sb.AppendLine($"Syntax: **!{command.Name}** {command.SyntaxHelp}");
                }
            }
            else
            {
                //var command = Commands.FirstOrDefault(x => !x.HideFromCommandList 
                //    && (!x.DebugOnly || _config.Debug)
                //    && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                var command = Commands.FirstOrDefault(x => !x.HideFromCommandList
                    && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (command == null)
                {
                    sb.AppendLine($"**The specified command does not exist!**");
                }
                else
                {
                    var rrc = command as IRoleRestrictedCommand;
                    if (rrc != null && (!e.Channel.IsPrivate || (rrc.ForRoles != null && rrc.ForRoles.Length > 0
                        && !_discord.Servers.Any(x => x.Roles.Any(y => y != null && rrc.ForRoles.Contains(y.Name, StringComparer.OrdinalIgnoreCase) == true && y.Members.Any(z => z.Id == e.User.Id))))))
                    {
                        sb.AppendLine($"**The specified command is only available to some roles and using a private channel.**");
                    }
                    else if (command.UsageExamples == null || command.UsageExamples.Length <= 0)
                    {
                        sb.AppendLine($"**The specified command does not have any usage examples :(**");
                    }
                    else
                    {
                        sb.AppendLine($"**Example usage of !{name.ToLower()}**");
                        foreach(var usageExample in command.UsageExamples)
                        {
                            if (string.IsNullOrWhiteSpace(usageExample)) continue;

                            sb.AppendLine($"● **!{command.Name}**" + (usageExample[0] == ':' ? "" : " ") + usageExample);
                        }
                    }
                }
                
            }

            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
        }
    }
}
