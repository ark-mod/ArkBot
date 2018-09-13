using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Database;
using ArkBot.Discord.Command;
using ArkBot.OpenID;
using ArkBot.Services;
using Discord;
using Discord.Commands.Builders;
using Discord.Net;

namespace ArkBot.Commands
{
    public class LinkSteamCommand : ModuleBase<SocketCommandContext>
    {
        private IConstants _constants;
        private IBarebonesSteamOpenId _openId;
        private EfDatabaseContextFactory _databaseContextFactory;

        public LinkSteamCommand(IConstants constants, IBarebonesSteamOpenId openId,
            EfDatabaseContextFactory databaseContextFactory)
        {
            _constants = constants;
            _openId = openId;
            _databaseContextFactory = databaseContextFactory;
        }

        [Command("linksteam")]
        [Summary("Link your Discord user with your Steam account")]
        [SyntaxHelp(null)]
        [UsageExamples(null)]
        [RoleRestrictedPrecondition("linksteam")]
        public async Task LinkSteam()
        {
            using (var context = _databaseContextFactory.Create())
            {
                if (context.Users.FirstOrDefault(x =>
                        x != null && x.DiscordId == (long)Context.User.Id && !x.Unlinked) != null)
                {
                    await Context.Channel.SendMessageAsync(
                        $"<@{Context.User.Id}>, your user is already linked with Steam. If you wish to remove this link use the command **!unlinksteam**.");
                    return;
                }
            }

            var state = await _openId.LinkWithSteamTaskAsync(Context.User.Id);
            if (state == null)
            {
                await Context.Channel.SendMessageAsync(
                    $"<@{Context.User.Id}>, something went wrong... :( Please try sending the **!linksteam** command to me in a private conversation instead!");
                return;
            }

            EmbedBuilder builder = new EmbedBuilder()
            {
                Title = "Steam Account Link",
                Description = $"Proceed to link your Discord user with your Steam account by [Logging into Steam]({state.StartUrl})",
                Url = state.StartUrl,
                Color = Color.Green

            };

            var channel = await Context.User.GetOrCreateDMChannelAsync();
            var msg = await channel.SendMessageAsync("", false, builder.Build());

            if (Context.IsPrivate) return;

            await Context.Channel.SendMessageAsync(
                $"<@{Context.User.Id}>, I have sent you a private message with instructions on how to proceed with linking your Discord user with Steam! If you do not receive this message, please try sending the **!linksteam** command to me in a private conversation!");

            //todo: how to get state in 1.0?
            //if (msg.State == MessageState.Normal || msg.State == MessageState.Queued)
            //    await Context.Channel.SendMessageAsync(
            //        $"<@{Context.User.Id}>, I have sent you a private message with instructions on how to proceed with linking your Discord user with Steam! If you do not receive this message, please try sending the **!linksteam** command to me in a private conversation!");
            //else
            //    await Context.Channel.SendMessageAsync(
            //        $"<@{Context.User.Id}>, it seems that I am unable to start a private conversation with you! :( Please try sending the **!linksteam** command to me in a private conversation instead!");
        }
    }
}
