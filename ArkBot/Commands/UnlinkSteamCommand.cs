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
using Discord;

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

        private IConstants _constants;
        private EfDatabaseContextFactory _databaseContextFactory;
        private DiscordClient _discord;

        public UnlinkSteamCommand(IConstants constants, EfDatabaseContextFactory databaseContextFactory)
        {
            _constants = constants;
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
                var user = context.Users.FirstOrDefault(x => x.DiscordId == (long)e.User.Id);
                if (user == null)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, your user is not linked with Steam.");
                }
                else
                {
                    foreach(var played in user.Played)
                    {
                        played.SteamId = user.SteamId;
                    }

                    context.Users.Remove(user);
                    var result = context.SaveChanges();

                    //remove ark role from users when they link
                    if (_discord?.Servers != null)
                    {
                        foreach (var server in _discord.Servers)
                        {
                            try
                            {
                                var duser = server.GetUser(e.User.Id);
                                var role = server.FindRoles("ark", true).FirstOrDefault();
                                if (duser != null && role == null) continue;

                                if (duser.HasRole(role)) await duser.RemoveRoles(role);
                            }
                            catch(Discord.Net.HttpException)
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
