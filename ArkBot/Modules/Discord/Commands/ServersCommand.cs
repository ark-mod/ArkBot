using ArkBot.Modules.Application;
using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.Discord.Attributes;
using ArkBot.Utils.Helpers;
using Discord;
using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArkBot.Modules.Discord.Commands
{
    public class ServersCommand : ModuleBase<SocketCommandContext>
    {
        private IConfig _config;
        private ArkContextManager _contextManager;

        public ServersCommand(IConfig config, ArkContextManager contextManager)
        {
            _config = config;
            _contextManager = contextManager;
        }

        [Command("servers")]
        [Summary("List the available servers")]
        [SyntaxHelp(null)]
        [UsageExamples(null)]
        [RoleRestrictedPrecondition("servers")]
        public async Task Servers([Remainder] string arguments = null)
        {
            var args = CommandHelper.ParseArgs(arguments, new { cluster = false, clusters = false }, x =>
                x.For(y => y.cluster, flag: true)
                    .For(y => y.clusters, flag: true));

            if (_config.Servers != null)
            {
                var embed = new EmbedBuilder();
                embed.WithTitle("Server List");

                foreach (var server in _config.Servers)
                {
                    var serverContext = _contextManager.GetServer(server.Key);

                    var address = server.DisplayAddress ?? $"{server.Ip}:{server.QueryPort}";

                    var cluster = args.cluster || args.clusters ? $" (cluster **`{server.ClusterKey}`**)" : "";

                    embed.AddField($"{ address}", $"steam://connect/{address} (key: `{server.Key}`){cluster}", true);
                }

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                await Context.Channel.SendMessageAsync("There are no servers available.");
                return;
            }
        }
    }
}
