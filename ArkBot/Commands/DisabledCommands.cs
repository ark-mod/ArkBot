using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Helpers;
using ArkBot.Extensions;
using System.Text.RegularExpressions;
using QueryMaster.GameServer;
using ArkBot.Database;
using Discord;
using ArkBot.Ark;

namespace ArkBot.Commands
{
    public class DisabledCommands : ICommand
    {
        public string Name => "disabled";
        public string[] Aliases => new string[] { "players", "playersx", "playerlist", "playerslist", "findtame",
            "findtames", "findpet", "findpets", "checkfood", "food", "mydinos", "mykibbles", "myeggs", "myresources", "mystuff", "myitems",
            "stats", "statistics", "top", "status", "serverstatus", "server", "vote", "votes", "voting" };
        public string Description => null;
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => true;

        public void Register(CommandBuilder command)
        {
            command.Parameter("optional", ParameterType.Multiple);
        }

        public void Init(DiscordClient client) { }

        private IConfig _config;

        public DisabledCommands(IConfig config)
        {
            _config = config;
        }

        public async Task Run(CommandEventArgs e)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"**This command is currently disabled.{(!string.IsNullOrWhiteSpace(_config.AppUrl) ? $" Please use {_config.AppUrl} as a substitute!" : "")}**");

            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
        }
    }
}