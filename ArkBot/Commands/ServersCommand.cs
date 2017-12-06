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
        public async Task Servers([Remainder] string arguments = null)
        {
            var args = CommandHelper.ParseArgs(arguments, new { cluster = false, clusters = false }, x =>
                x.For(y => y.cluster, flag: true)
                    .For(y => y.clusters, flag: true));

            if (_config.Servers != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("**Server List**");

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

                    var address = $"{server.Ip}:{server.Port}";

                    var cluster = args.cluster || args.clusters ? $" (cluster **{server.Cluster}**)" : "";

                    sb.AppendLine(
                        $"● **{name ?? address}**{(name != null ? $" ({address})" : "")} (key: **{server.Key}**){cluster}");
                }

                await CommandHelper.SendPartitioned(Context.Channel, sb.ToString());
            }
            else
            {
                await Context.Channel.SendMessageAsync("There are no servers available.");
                return;
            }
        }
    }
}
