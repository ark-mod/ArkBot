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
    public class PlayerListCommand : ICommand
    {
        public string Name => "players";
        public string[] Aliases => new[] { "playerlist", "playerslist" };
        public string Description => "List of players currently in-game";
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        public void Register(CommandBuilder command) { }

        public async Task Run(CommandEventArgs e)
        {
            var status = await CommandHelper.GetServerStatus();

            var sb = new StringBuilder();
            if (status == null || status.Item1 == null || status.Item3 == null)
            {
                sb.AppendLine($"**Player list is currently unavailable!**");
            }
            else
            {
                var serverInfo = status.Item1;
                var playerInfo = status.Item3;
                var players = playerInfo?.Where(x => !string.IsNullOrEmpty(x.Name)).ToArray() ?? new PlayerInfo[] { };

                var m = new Regex(@"^(?<name>.+?)\s+-\s+\(v(?<version>\d+\.\d+)\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(serverInfo.Name);
                var name = m.Success ? m.Groups["name"].Value : serverInfo.Name;

                sb.AppendLine($"**{name} ({serverInfo.Players - (playerInfo.Count - players.Length)}/{serverInfo.MaxPlayers})**");
                foreach (var player in players)
                {
                    sb.AppendLine($"● **{player.Name}** ({player.Time.ToStringCustom()})");
                }
            }

            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
        }
    }
}