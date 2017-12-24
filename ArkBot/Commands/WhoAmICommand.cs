using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Extensions;
using ArkBot.Database;
using ArkBot.Discord.Command;
using Discord;
using Discord.Commands.Builders;
using Discord.Net;

namespace ArkBot.Commands
{
    public class WhoAmICommand : ModuleBase<SocketCommandContext>
    {
        private EfDatabaseContextFactory _databaseContextFactory;
        public WhoAmICommand(IConstants constants, EfDatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
        }

        [Command("whoami")]
        [Summary("Find out what we know about you")]
        [SyntaxHelp(null)]
        [UsageExamples(null)]
        [RoleRestrictedPrecondition("whoami")]
        public async Task Whoami()
        {
            using (var context = _databaseContextFactory.Create())
            {
                var user = context.Users.FirstOrDefault(x => x.DiscordId == (long)Context.User.Id && !x.Unlinked);
                if (user == null)
                {
                    await Context.Channel.SendMessageAsync($"<@{Context.User.Id}>, your existence is a mystery to us! :(");
                }
                else
                {
                    if (!Context.IsPrivate) await Context.Channel.SendMessageAsync($"<@{Context.User.Id}>, I will send you a private message with everything we know about you!");

                    var sb = new StringBuilder();
                    sb.AppendLine($"**This is what we know about you:**");
                    sb.AppendLine($"● **Discord ID:** {user.DiscordId}");
                    sb.AppendLine($"● **Steam ID:** {user.SteamId}");
                    if (user.SteamDisplayName != null) sb.AppendLine($"● **Steam nick:** {user.SteamDisplayName}");
                    if (user.RealName != null) sb.AppendLine($"● **Real name:** {user.RealName}");

                    var channel = await Context.User.GetOrCreateDMChannelAsync();
                    foreach (var msg in sb.ToString().Partition(2000))
                    {
                        await channel.SendMessageAsync(msg.Trim('\r', '\n'));
                    }
                }
            }
        }
    }
}
