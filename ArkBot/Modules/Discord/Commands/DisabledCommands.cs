using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.Discord.Attributes;
using ArkBot.Utils.Helpers;
using Discord.Commands;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Modules.Discord.Commands
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

            sb.AppendLine($"**This command is currently disabled.{(!string.IsNullOrWhiteSpace(_config.WebApp.ExternalUrl) ? $" Please use {_config.WebApp.ExternalUrl} as a substitute!" : "")}**");

            await CommandHelper.SendPartitioned(Context.Channel, sb.ToString());
        }
    }
}