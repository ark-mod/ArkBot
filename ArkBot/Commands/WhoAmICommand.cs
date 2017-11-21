using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Extensions;
using ArkBot.Database;
using Discord;

namespace ArkBot.Commands
{
    public class WhoAmICommand : ICommand
    {
        public string Name => "whoami";
        public string[] Aliases => null;
        public string Description => "Find out what we know about you";
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        private EfDatabaseContextFactory _databaseContextFactory;

        public WhoAmICommand(IConstants constants, EfDatabaseContextFactory databaseContextFactory)
        {
            _databaseContextFactory = databaseContextFactory;
        }

        public void Register(CommandBuilder command)
        {
            //command.AddCheck((cmd, usr, ch) =>
            //    {
            //        return ch.IsPrivate;
            //    });
        }

        public void Init(DiscordClient client) { }

        public async Task Run(CommandEventArgs e)
        {
            using (var context = _databaseContextFactory.Create())
            {
                var user = context.Users.FirstOrDefault(x => x.DiscordId == (long)e.User.Id && !x.Unlinked);
                if (user == null)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, your existence is a mystery to us! :(");
                }
                else
                {
                    if(!e.Channel.IsPrivate) await e.Channel.SendMessage($"<@{e.User.Id}>, I will send you a private message with everything we know about you!");

                    var sb = new StringBuilder();
                    sb.AppendLine($"**This is what we know about you:**");
                    sb.AppendLine($"● **Discord ID:** {user.DiscordId}");
                    sb.AppendLine($"● **Steam ID:** {user.SteamId}");
                    if (user.SteamDisplayName != null) sb.AppendLine($"● **Steam nick:** {user.SteamDisplayName}");
                    if (user.RealName != null) sb.AppendLine($"● **Real name:** {user.RealName}");

                    if (e.User.PrivateChannel == null) await e.User.CreatePMChannel();
                    foreach (var msg in sb.ToString().Partition(2000))
                    {
                        await e.User.PrivateChannel.SendMessage(msg.Trim('\r', '\n'));
                    }
                }
            }
        }
    }
}
