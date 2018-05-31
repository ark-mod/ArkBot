using System;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Helpers;
using System.Reflection;
using ArkBot.Discord.Command;
using ArkBot.Configuration.Model;

namespace ArkBot.Commands
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
        [Summary("Get a link to the Companion App (Web App)")]
        [RoleRestrictedPrecondition("webapp")]
        public async Task WebApp()
        {
            var sb = new StringBuilder();
            if (string.IsNullOrWhiteSpace(_config.AppUrl))
                sb.AppendLine("**The setting `appUrl` is missing from the configuration...**");
            else
            {
                sb.AppendLine("**Companion App (Web App)**");
                sb.AppendLine($"{_config.AppUrl}");
            }
            await CommandHelper.SendPartitioned(Context.Channel, sb.ToString());
        }
    }
}
