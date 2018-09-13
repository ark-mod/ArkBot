using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Discord.Command;
using Discord.Commands;
using ArkBot.Helpers;
using Discord;
using Discord.Commands.Builders;
using Discord.Net;
using RestSharp;
using ArkBot.Configuration.Model;

namespace ArkBot.Commands
{
    public class DisabledCommands : ModuleBase<SocketCommandContext>
    {
        private IConfig _config;

        public DisabledCommands(IConfig config)
        {
            _config = config;
        }

        [CommandHidden]
        [Command("disabled")]
        [Alias("players", "playersx", "playerlist", "playerslist",
            "findtame", "findtames", "findpet", "findpets",
            "checkfood", "food", "mydinos", "mykibbles", "myeggs", "myresources", "mystuff", "myitems",
            "stats", "statistics", "top", "status", "serverstatus", "server", "vote", "votes", "voting")]
        [Summary("Unlink your Discord user from your Steam account")]
        [SyntaxHelp(null)]
        [UsageExamples(null)]
        [RoleRestrictedPrecondition("disabled")]
        public async Task Disabled([Remainder] string arguments = null)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"**This command is currently disabled.{(!string.IsNullOrWhiteSpace(_config.AppUrl) ? $" Please use {_config.AppUrl} as a substitute!" : "")}**");

            await CommandHelper.SendPartitioned(Context.Channel, sb.ToString());
        }
    }
}