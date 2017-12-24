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
        public string AccessControlName { get; set; }

        public RoleRestrictedPreconditionAttribute(string accessControlName)
        {
            AccessControlName = accessControlName;
        }

        // Override the CheckPermissions method
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            try
            {
                var config = (IConfig) services.GetService(typeof(IConfig));

                var roles = new List<string>();
                if (context.Channel is ISocketPrivateChannel)
                {
                    var guilds = await context.Client.GetGuildsAsync();
                    foreach (var guild in guilds)
                    {
                        var user = await guild.GetUserAsync(context.User.Id);
                        roles.AddRange(guild.Roles.Where(x => user.RoleIds.Contains(x.Id)).Select(x => x.Name));
                    }
                }
                else
                {
                    var user = await context.Guild.GetUserAsync(context.User.Id);
                    roles.AddRange(context.Guild.Roles.Where(x => user.RoleIds.Contains(x.Id)).Select(x => x.Name));
                }

                roles = roles.Distinct().ToList();

                var hasFeatureAccess =
                    HasFeatureAccess(config, "commands", AccessControlName, roles.ToArray());
                if (!hasFeatureAccess)
                    return PreconditionResult.FromError("The user does not have access to this command!");

                var croles = GetFeatureRoles(config, "commands", AccessControlName)?.Intersect(roles).ToArray() ??
                             new string[] { };
                var proles =
                    GetFeatureRoles(config, "channels",
                        context.Channel is ISocketPrivateChannel ? "private" : "public") ?? new string[] { };

                return proles.Intersect(croles).Any()
                    ? PreconditionResult.FromSuccess()
                    : PreconditionResult.FromError(
                        $"The user must send this command in {(context.Channel is ISocketPrivateChannel ? "private" : "public")}!");
            }
            catch (Exception ex)
            {
                Logging.LogException("Exception when checking command permissions", ex, GetType(), LogLevel.ERROR, ExceptionLevel.Unhandled);
                return PreconditionResult.FromError($"Permission check failed with exception: {ex.Message}");
            }
        }

        private bool HasFeatureAccess(IConfig config, string featureGroup, string featureName, string[] roles)
        {
            var rf = GetFeatureRoles(config, featureGroup, featureName);
            return rf != null && rf.Intersect(roles, StringComparer.OrdinalIgnoreCase).Any();
        }

        private string[] GetFeatureRoles(IConfig config, string featureGroup, string featureName)
        {
            if (featureGroup == null) return null;
            if (featureName == null) return null;

            var accessControl = config.Discord?.AccessControl;
            if (accessControl == null) return null;
            if (!accessControl.TryGetValue(featureGroup, out var fg)) return null;

            return !fg.TryGetValue(featureName, out var rf) ? null : rf;
        }
    }
}
