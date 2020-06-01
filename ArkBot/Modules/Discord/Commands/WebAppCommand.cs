using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.Discord.Attributes;
using ArkBot.Utils.Helpers;
using Discord.Commands;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Modules.Discord.Commands
{
    public class WebAppCommand : ModuleBase<SocketCommandContext>
    {
        private IConfig _config;

        public WebAppCommand(IConfig config)
        {
            _config = config;
        }

        [Command("webapp")]
        [Alias("app", "companionapp")]
        [Summary("Get a link to the Web App")]
        [RoleRestrictedPrecondition("webapp")]
        public async Task WebApp()
        {
            var sb = new StringBuilder();
            if (string.IsNullOrWhiteSpace(_config.WebApp.ExternalUrl))
                sb.AppendLine("**The setting `webApp.appUrl` is missing from the configuration...**");
            else
            {
                sb.AppendLine("**Web App**");
                sb.AppendLine($"{_config.WebApp.ExternalUrl}");
            }
            await CommandHelper.SendPartitioned(Context.Channel, sb.ToString());
        }
    }
}
