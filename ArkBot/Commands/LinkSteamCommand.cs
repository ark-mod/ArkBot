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
using ArkBot.Database;
using ArkBot.OpenID;
using ArkBot.Services;

namespace ArkBot.Commands
{
    public class LinkSteamCommand : ICommand
    {
        public string Name => "linksteam";
        public string[] Aliases => null;
        public string Description => "Link your Discord user with your Steam account";
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        private IConstants _constants;
        private IBarebonesSteamOpenId _openId;
        private IUrlShortenerService _urlShortenerService;
        private DatabaseContextFactory<IEfDatabaseContext> _databaseContextFactory;

        public LinkSteamCommand(IConstants constants, IBarebonesSteamOpenId openId, IUrlShortenerService urlShortenerService, DatabaseContextFactory<IEfDatabaseContext> databaseContextFactory)
        {
            _constants = constants;
            _openId = openId;
            _urlShortenerService = urlShortenerService;
            _databaseContextFactory = databaseContextFactory;
        }

        public void Register(CommandBuilder command) { }

        public async Task Run(CommandEventArgs e)
        {
            using (var context = _databaseContextFactory.Create())
            {
                if (context.Users.FirstOrDefault(x => x.DiscordId == (long)e.User.Id) != null)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, your user is already linked with Steam. If you wish to remove this link use the command **!unlinksteam**.");
                    return;
                }
            }

            var state = await _openId.LinkWithSteamTaskAsync(e.User.Id);
            var sb = new StringBuilder();
            sb.AppendLine($"**Proceed to link your Discord user with your Steam account by following this link:**");
            sb.AppendLine($"{await _urlShortenerService.ShortenUrl(state.StartUrl)}");
            var msg = await e.User.PrivateChannel.SendMessage(sb.ToString().Trim('\r', '\n'));

            if (e.User.PrivateChannel == e.Channel) return;

            if (msg.State == Discord.MessageState.Normal || msg.State == Discord.MessageState.Queued)
                await e.Channel.SendMessage($"<@{e.User.Id}>, I have sent you a private message with instructions on how to proceed with linking your Discord user with Steam! If you do not receive this message, please try sending the **!linksteam** command to me in a private conversation!");
            else
                await e.Channel.SendMessage($"<@{e.User.Id}>, it seems that I am unable to start a private conversation with you! :( Please try sending the **!linksteam** command to me in a private conversation instead!");
        }
    }
}
