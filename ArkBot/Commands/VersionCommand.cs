using System;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Helpers;
using System.Reflection;
using ArkBot.Discord.Command;

namespace ArkBot.Commands
{
    public class VersionCommand : ModuleBase<SocketCommandContext>
    {
        [Command("version")]
        [Summary("Get the bot version number")]
        [RoleRestrictedPrecondition("version")]
        public async Task Version()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"**My operational instructions indicate that I am version {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}. But what does it mean?**");
            await CommandHelper.SendPartitioned(Context.Channel, sb.ToString());
        }
    }
}
