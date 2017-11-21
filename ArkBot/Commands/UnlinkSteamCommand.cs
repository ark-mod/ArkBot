using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Database;
using Discord;
using Discord.Net;

namespace ArkBot.Commands
{
    public class UnlinkSteamCommand : ICommand
    {
        public string Name => "unlinksteam";
        public string[] Aliases => null;
        public string Description => "Unlink your Discord user from your Steam account";
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        private IConfig _config;
        private EfDatabaseContextFactory _databaseContextFactory;
        private DiscordClient _discord;

        public UnlinkSteamCommand(IConstants constants, IConfig config, EfDatabaseContextFactory databaseContextFactory)
        {
            _config = config;
            _databaseContextFactory = databaseContextFactory;
        }

        public void Register(CommandBuilder command) { }

        public void Init(DiscordClient client)
        {
            _discord = client;
        }

        public async Task Run(CommandEventArgs e)
        {
            using (var context = _databaseContextFactory.Create())
            {
                var user = context.Users.FirstOrDefault(x => x.DiscordId == (long)e.User.Id && !x.Unlinked);
                if (user == null)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, your user is not linked with Steam.");
                }
                else
                {
                    user.Unlinked = true;
                    var result = context.SaveChanges();

                    //remove ark role from users when they unlink
                    if (_discord?.Servers != null)
                    {
                        foreach (var server in _discord.Servers)
                        {
                            try
                            {
                                var duser = server.GetUser(e.User.Id);
                                var role = server.FindRoles(_config.MemberRoleName, true).FirstOrDefault();
                                if (duser != null && role == null) continue;

                                if (duser?.HasRole(role) == true) await duser.RemoveRoles(role);
                            }
                            catch(HttpException)
                            {
                                //could be due to the order of roles on the server. bot role with "manage roles" permission must be higher up than the role it is trying to set
                            }
                        }
                    }

                    await e.Channel.SendMessage($"<@{e.User.Id}>, your user is no longer linked with Steam.");
                }
            }
        }
    }
}
