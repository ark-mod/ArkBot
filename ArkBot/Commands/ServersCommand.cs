using System.Linq;
using ArkBot.Ark;
using ArkBot.Helpers;
using Discord;
using Discord.Commands;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArkBot.Discord.Command;
using Discord.Commands.Builders;
using Discord.Net;
using ArkBot.Configuration.Model;

namespace ArkBot.Commands
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
                    var info = serverContext.Steam.GetServerInfoCached();
                    string name = null;
                    if (info != null)
                    {
                        var m = new Regex(@"^(?<name>.+?)\s+-\s+\(v(?<version>\d+\.\d+)\)$",
                            RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(info.Name);
                        name = m.Success ? m.Groups["name"].Value : info.Name;
                    }

                    var address = server.DisplayAddress ?? $"{server.Ip}:{server.QueryPort}";

                    var cluster = args.cluster || args.clusters ? $" (cluster **`{server.ClusterKey}`**)" : "";

                    embed.AddField($"{ name ?? address}", $"steam://connect/{address} (key: `{server.Key}`){cluster}",true);
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
