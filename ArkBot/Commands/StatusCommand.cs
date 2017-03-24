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
using System.Text.RegularExpressions;
using QueryMaster.GameServer;
using System.Runtime.Caching;

namespace ArkBot.Commands
{
    public class StatusCommand : ICommand
    {
        public string Name => "status";
        public string[] Aliases => new[] { "serverstatus", "server" };
        public string Description => "Get the current server status";
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        private IArkContext _context;
        private IConfig _config;

        public StatusCommand(IArkContext context, IConfig config)
        {
            _context = context;
            _config = config;
        }

        public void Register(CommandBuilder command) { }

        public void Init(Discord.DiscordClient client) { }

        public async Task Run(CommandEventArgs e)
        {
            var status = await CommandHelper.GetServerStatus(_config);

            var sb = new StringBuilder();
            if (status == null || status.Item1 == null || status.Item2 == null)
            {
                sb.AppendLine($"**Server status is currently unavailable!**");
            }
            else
            {
                var serverInfo = status.Item1;
                var serverRules = status.Item2;

                var m = new Regex(@"^(?<name>.+?)\s+-\s+\(v(?<version>\d+\.\d+)\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(serverInfo.Name);
                var name = m.Success ? m.Groups["name"].Value : serverInfo.Name;
                var version = m.Success ? m.Groups["version"] : null;
                var currentTime = serverRules.FirstOrDefault(x => x.Name == "DayTime_s")?.Value;
                var tamedDinosCount = _context.Creatures?.Count();
                var wildDinosCount = _context.Wild?.Count();
                var tamedDinosMax = 6000; //todo: remove hardcoded value
                var structuresCount = _context.Tribes?.SelectMany(x => x.Structures).Sum(x => x.Count);
                var totalPlayers = _context.Players?.Count();
                var totalTribes = _context.Tribes?.Count();

                sb.AppendLine($"**{name}**");
                sb.AppendLine($"● **Address:** {serverInfo.Address}");
                if (version != null) sb.AppendLine($"● **Version:** {version}");
                sb.AppendLine($"● **Online:** {serverInfo.Players}/{serverInfo.MaxPlayers}");
                sb.AppendLine($"● **Map:** {serverInfo.Map}");
                if (currentTime != null) sb.AppendLine($"● **In-game time:** {currentTime}");

                sb.AppendLine().AppendLine($"**Server Statistics**");
                if (tamedDinosCount.HasValue) sb.AppendLine($"● **Tamed dinos:** {tamedDinosCount.Value:N0}/{tamedDinosMax:N0}");
                if (wildDinosCount.HasValue) sb.AppendLine($"● **Wild dinos:** {wildDinosCount.Value:N0}");
                if (structuresCount.HasValue) sb.AppendLine($"● **Structures:** {structuresCount.Value:N0}");
                if (totalPlayers.HasValue) sb.AppendLine($"● **Players:** {totalPlayers.Value:N0}");
                if (totalTribes.HasValue) sb.AppendLine($"● **Tribes:** {totalTribes.Value:N0}");

                var nextUpdate = _context.ApproxTimeUntilNextUpdate;
                var nextUpdateTmp = nextUpdate?.ToStringCustom();
                var nextUpdateString = (nextUpdate.HasValue ? (!string.IsNullOrWhiteSpace(nextUpdateTmp) ? $"~{nextUpdateTmp}" : "waiting for new update ...") : null);
                var lastUpdate = _context.LastUpdate;
                var lastUpdateString = lastUpdate.ToStringWithRelativeDay();

                sb.AppendLine($"● **Last update:** {lastUpdateString}");
                if(nextUpdateString != null) sb.AppendLine($"● **Next update:** {nextUpdateString}");

                
            }

            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
        }
    }
}
