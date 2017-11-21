using ArkBot.Ark;
using ArkBot.Helpers;
using Discord;
using Discord.Commands;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArkBot.Commands
{
    //public class ServersCommand
    //{
    //    //else if (args.keys)
    //    //    {
    //    //        if (_config.Servers != null)
    //    //        {
    //    //            var sb = new StringBuilder();
    //    //            sb.AppendLine("**Server instance keys**");

    //    //            foreach (var server in _config.Servers) sb.AppendLine($"● **{server.Key}** (cluster **{server.Cluster}**): {server.Ip}:{server.Port}");

    //    //            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
    //    //        }
    //    //        else
    //    //        {
    //    //            await e.Channel.SendMessage("There are no server instances configured.");
    //    //            return;
    //    //        }
    //    //    }
    //}

    public class ServersCommand : ICommand
    {
        public string Name => "servers";
        public string[] Aliases => null;
        public string Description => "List the available servers";
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        private IConfig _config;
        private ArkContextManager _contextManager;

        public ServersCommand(IConfig config, ArkContextManager contextManager)
        {
            _config = config;
            _contextManager = contextManager;
        }

        public void Register(CommandBuilder command) { }

        public void Init(DiscordClient client) { }

        public async Task Run(CommandEventArgs e)
        {
            var args = CommandHelper.ParseArgs(e, new { cluster = false, clusters = false }, x =>
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
                        var m = new Regex(@"^(?<name>.+?)\s+-\s+\(v(?<version>\d+\.\d+)\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(info.Name);
                        name = m.Success ? m.Groups["name"].Value : info.Name;
                    }

                    var address = $"{server.Ip}:{server.Port}";

                    var cluster = args.cluster || args.clusters ? $" (cluster **{server.Cluster}**)" : "";

                    sb.AppendLine($"● **{name ?? address}**{(name != null ? $" ({address})" : "")} (key: **{server.Key}**)");
                }

                await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
            }
            else
            {
                await e.Channel.SendMessage("There are no servers available.");
                return;
            }
        }
    }
}
