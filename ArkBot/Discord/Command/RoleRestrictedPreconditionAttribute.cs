using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Configuration.Model;
using Discord.Commands;
using Discord.WebSocket;

namespace ArkBot.Discord.Command
{
    public class RoleRestrictedPreconditionAttribute : PreconditionAttribute
    {
        internal const string CommandDisabledErrorString = "Command disabled";

        public string AccessControlName { get; set; }

        public RoleRestrictedPreconditionAttribute(string accessControlName)
        {
            AccessControlName = accessControlName;
        }

        // Override the CheckPermissions method
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
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

                var froles = GetFeatureRoles(config, "commands", AccessControlName);
                if (!(froles?.Length > 0)) return PreconditionResult.FromError(CommandDisabledErrorString);
                if (!HasFeatureAccess(froles, roles.ToArray()))
                    return PreconditionResult.FromError("The user does not have access to this command!");

                var croles = froles?.Intersect(roles).ToArray() ??
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

        private bool HasFeatureAccess(string[] featureRoles, string[] userRoles)
        {
            return featureRoles != null && featureRoles.Intersect(userRoles, StringComparer.OrdinalIgnoreCase).Any();
        }

        private string[] GetFeatureRoles(IConfig config, string featureGroup, string featureName)
        {
            if (featureGroup == null) return null;
            if (featureName == null) return null;

            var accessControl = config.Discord?.AccessControl;
            if (accessControl == null) return null;
            if (!accessControl.TryGetValue(featureGroup, out var fg)) return null;

            return !fg.TryGetValue(featureName, out var rf) ? null : rf.ToArray();
        }
    }
}
