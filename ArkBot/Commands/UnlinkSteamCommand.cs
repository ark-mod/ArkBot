using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using ArkBot.Database;
using ArkBot.Discord.Command;
using Discord;
using Discord.Net;
using ArkBot.Configuration.Model;

namespace ArkBot.Commands
{
    public class UnlinkSteamCommand : ModuleBase<SocketCommandContext>
    {
        private IConfig _config;
        private EfDatabaseContextFactory _databaseContextFactory;

        public UnlinkSteamCommand(IConstants constants, IConfig config, EfDatabaseContextFactory databaseContextFactory)
        {
            _config = config;
            _databaseContextFactory = databaseContextFactory;
        }

        [Command("unlinksteam")]
        [Summary("Unlink your Discord user from your Steam account")]
        [SyntaxHelp(null)]
        [UsageExamples(null)]
        [RoleRestrictedPrecondition("unlinksteam")]
        public async Task UnlinkSteam()
        {
            using (var context = _databaseContextFactory.Create())
            {
                var user = context.Users.FirstOrDefault(x => x.DiscordId == (long)Context.User.Id && !x.Unlinked);
                if (user == null)
                {
                    await Context.Channel.SendMessageAsync($"<@{Context.User.Id}>, your user is not linked with Steam.");
                }
                else
                {
                    user.Unlinked = true;
                    var result = context.SaveChanges();

                    //remove ark role from users when they unlink
                    if (Context.Client?.Guilds != null)
                    {
                        foreach (var server in Context.Client.Guilds)
                        {
                            try
                            {
                                var duser = server.GetUser(Context.User.Id);
                                var role = server.Roles.FirstOrDefault(x => x.Name.Equals(_config.Discord.MemberRoleName));
                                if (duser != null && role == null) continue;

                                if (duser?.Roles.Any(x => x.Id == role.Id) == true) await duser.RemoveRoleAsync(role);
                            }
                            catch (HttpException)
                            {
                                //could be due to the order of roles on the server. bot role with "manage roles" permission must be higher up than the role it is trying to set
                            }
                        }
                    }

                    await Context.Channel.SendMessageAsync($"<@{Context.User.Id}>, your user is no longer linked with Steam.");
                }
            }
        }
    }
}
