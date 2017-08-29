//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Discord.Commands;
//using ArkBot.Helpers;
//using ArkBot.Extensions;
//using static System.FormattableString;
//using System.Drawing;
//using System.Text.RegularExpressions;
//using QueryMaster.GameServer;
//using System.Runtime.Caching;
//using System.Diagnostics;
//using Discord;
//using ArkBot.Ark;

//namespace ArkBot.Commands
//{
//    public class StatusCommand : ICommand
//    {
//        public string Name => "status";
//        public string[] Aliases => new[] { "serverstatus", "server" };
//        public string Description => "Get the current server status";
//        public string SyntaxHelp => null;
//        public string[] UsageExamples => null;

//        public bool DebugOnly => false;
//        public bool HideFromCommandList => false;

//        private IConfig _config;
//        private IConstants _constants;
//        private ArkContextManager _contextManager;

//        public StatusCommand(IConfig config, IConstants constants, ArkContextManager contextManager)
//        {
//            _config = config;
//            _constants = constants;
//            _contextManager = contextManager;
//        }

//        public void Register(CommandBuilder command) { }

//        public void Init(DiscordClient client) { }

//        public async Task Run(CommandEventArgs e)
//        {
//            if (!_context.IsInitialized)
//            {
//                await e.Channel.SendMessage($"**The data is loading but is not ready yet...**");
//                return;
//            }

//            var serverContext = _contextManager.GetServer(_config.ServerKey);
//            var info = serverContext.Steam.GetServerInfoCached();
//            var rules = serverContext.Steam.GetServerRulesCached();

//            var sb = new StringBuilder();
//            if (info == null || rules == null)
//            {
//                sb.AppendLine($"**Server status is currently unavailable!**");
//            }
//            else
//            {
//                var m = new Regex(@"^(?<name>.+?)\s+-\s+\(v(?<version>\d+\.\d+)\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(info.Name);
//                var name = m.Success ? m.Groups["name"].Value : info.Name;
//                var version = m.Success ? m.Groups["version"] : null;
//                var currentTime = rules.FirstOrDefault(x => x.Name == "DayTime_s")?.Value;
//                var tamedDinosCount = _context.Creatures?.Count();
//                var uploadedDinosCount = _context.Cluster?.Creatures?.Count();
//                var wildDinosCount = _context.Wild?.Count();
//                var tamedDinosMax = 6000; //todo: remove hardcoded value
//                var structuresCount = _context.Tribes?.SelectMany(x => x.Structures).Sum(x => x.Count);
//                var totalPlayers = _context.Players?.Count();
//                var totalTribes = _context.Tribes?.Count();

//                //server uptime
//                DateTime? serverStarted = null;
//                try
//                {
//                    serverStarted = Process.GetProcessesByName(_constants.ArkServerProcessName)?.FirstOrDefault()?.StartTime;

//                }
//                catch { /* ignore exceptions */ }

//                sb.AppendLine($"**{name}**");
//                sb.AppendLine($"● **Address:** {info.Address}");
//                if (version != null) sb.AppendLine($"● **Version:** {version}");
//                sb.AppendLine($"● **Online:** {info.Players}/{info.MaxPlayers}");
//                sb.AppendLine($"● **Map:** {info.Map}");
//                if (currentTime != null) sb.AppendLine($"● **In-game time:** {currentTime}");
//                if (serverStarted != null) sb.AppendLine($"Server uptime: {(DateTime.Now - serverStarted.Value).ToStringCustom(true)}");

//                sb.AppendLine().AppendLine($"**Server Statistics**");
//                if (tamedDinosCount.HasValue) sb.AppendLine($"● **Tamed dinos:** {tamedDinosCount.Value:N0}/{tamedDinosMax:N0}");
//                if (uploadedDinosCount.HasValue) sb.AppendLine($"● **Uploaded dinos:** {uploadedDinosCount.Value:N0}");
//                if (wildDinosCount.HasValue) sb.AppendLine($"● **Wild dinos:** {wildDinosCount.Value:N0}");
//                if (structuresCount.HasValue) sb.AppendLine($"● **Structures:** {structuresCount.Value:N0}");
//                if (totalPlayers.HasValue) sb.AppendLine($"● **Players:** {totalPlayers.Value:N0}");
//                if (totalTribes.HasValue) sb.AppendLine($"● **Tribes:** {totalTribes.Value:N0}");

//                var nextUpdate = _context.ApproxTimeUntilNextUpdate;
//                var nextUpdateTmp = nextUpdate?.ToStringCustom();
//                var nextUpdateString = (nextUpdate.HasValue ? (!string.IsNullOrWhiteSpace(nextUpdateTmp) ? $"~{nextUpdateTmp}" : "waiting for new update ...") : null);
//                var lastUpdate = _context.LastUpdate;
//                var lastUpdateString = lastUpdate.ToStringWithRelativeDay();

//                sb.AppendLine($"● **Last update:** {lastUpdateString}");
//                if (nextUpdateString != null) sb.AppendLine($"● **Next update:** {nextUpdateString}");

                
//            }

//            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
//        }
//    }
//}
