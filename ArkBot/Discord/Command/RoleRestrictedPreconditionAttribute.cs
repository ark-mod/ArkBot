using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace ArkBot.Discord.Command
{
    public class RoleRestrictedPreconditionAttribute : PreconditionAttribute
    {
        public DiscordRole[] ForRoles { get; set; }

        public RoleRestrictedPreconditionAttribute(DiscordRole[] forRoles)
        {
            ForRoles = forRoles;
        }

        // Override the CheckPermissions method
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (!(context.Channel is ISocketPrivateChannel)) return PreconditionResult.FromError("Role restricted commands must be sent in private!");
            if (!(ForRoles?.Length > 0)) return PreconditionResult.FromSuccess();

            var config = (IConfig)services.GetService(typeof(IConfig));
            var forRoles = ForRoles.Select(x => config.TranslateDiscordRoleName(x)).ToArray();

            var guilds = await context.Client.GetGuildsAsync();

            foreach (var guild in guilds)
            {
                foreach (var role in guild.Roles)
                {
                    if (role == null || !forRoles.Contains(role.Name, StringComparer.OrdinalIgnoreCase)) continue;

                    var user = await guild.GetUserAsync(context.User.Id);
                    if (user.RoleIds.Contains(role.Id)) return PreconditionResult.FromSuccess();
                }
            }

            return PreconditionResult.FromError("The user does not have access to this command!");
        }
    }
}
