//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Discord.Commands;
//using ArkBot.Helpers;
//using ArkBot.Extensions;
//using static System.FormattableString;
//using System.Drawing;
//using System.Text.RegularExpressions;
//using QueryMaster.GameServer;
//using System.Runtime.Caching;
//using System.Reflection;
//using ArkBot.Database;
//using ArkBot.Database.Model;
//using System.ComponentModel;
//using ArkBot.Ark;
//using Discord;
//using ArkBot.Voting.Handlers;
//using ArkBot.Voting;

//namespace ArkBot.Commands
//{
//    public class VoteCommand : ICommand // : IEnabledCheckCommand
//    {
//        public string Name => "vote";
//        public string[] Aliases => new[] { "votes", "voting" };
//        public string Description => "Community voting system";
//        public string SyntaxHelp => "<***vote identifier***> <***yes/no***>";
//        public string[] UsageExamples => new[]
//        {
//            ": List all votes that are underway",
//            "**alpha yes**: Vote ***yes*** in the vote identified by ***alpha***",
//            "**echo no reason \"This vote is not in accordance with the rules\"**: Vote ***no*** in the vote identified by ***echo*** and give a reason that can be viewed in the logs",
//            "**<serverkey> ban24h <name> reason <text>**: Start a vote to ban a user for 24h. Reason is logged and you are responsible for acting in accordance with the server rules.",
//            "**<serverkey> ban <name> reason <text>**: Start a vote to ban a user. Reason is logged and you are responsible for acting in accordance with the server rules.",
//            "**<serverkey> unban <name> reason <text>**: Start a vote to unban a user. Reason is logged and you are responsible for acting in accordance with the server rules.",
//            "**<serverkey> destroywilddinos reason <text>**: Start a vote to execute a wild dino wipe. Reason is logged and you are responsible for acting in accordance with the server rules.",
//            "**<serverkey> settimeofday <hh:mm:ss> reason <text>**: Start a vote to change the time of day. Reason is logged and you are responsible for acting in accordance with the server rules.",
//            "**<serverkey> restartserver reason <text>**: Start a vote to restart the server. Reason is logged and you are responsible for acting in accordance with the server rules.",
//            "**<serverkey> updateserver reason <text>**: Start a vote to update the server. Reason is logged and you are responsible for acting in accordance with the server rules."
//        };

//        public bool DebugOnly => false;
//        public bool HideFromCommandList => false;

//        //public bool EnabledCheck()
//        //{
//        //    return !string.IsNullOrWhiteSpace(_config.RconPassword) && _config.RconPort > 0;
//        //}

//        private IConfig _config;
//        private EfDatabaseContextFactory _databaseContextFactory;
//        private DiscordClient _discord;
//        private ISavedState _savedstate;
//        private ArkContextManager _contextManager;

//        public VoteCommand(IConfig config, EfDatabaseContextFactory databaseContextFactory, ISavedState savedstate, ArkContextManager contextManager)
//        {
//            _config = config;
//            _databaseContextFactory = databaseContextFactory;
//            _savedstate = savedstate;
//            _contextManager = contextManager;
//        }

//        public void Register(CommandBuilder command)
//        {
//            command.Parameter("optional", ParameterType.Multiple);
//        }

//        public void Init(DiscordClient client)
//        {
//            _discord = client;
//        }

//        private readonly string[] _identifierStrings = new[]
//            {
//                "Alfa", "Bravo", "Charlie", "Delta", "Echo", "Foxtrot", "Golf", "Hotel", "India",
//                "Juliett", "Kilo", "Lima", "Mike", "November", "Oscar", "Papa", "Quebec", "Romeo",
//                "Sierra", "Tango", "Uniform", "Victor", "Whiskey", "Xray", "Yankee", "Zulu"
//            };
//        private readonly string[] _voteStrings = new[] { "Yes", "No", "Veto" };

//        private static Random _rnd = new Random();

//        public async Task Run(CommandEventArgs e)
//        {
//            if (_savedstate.VotingDisabled)
//            {
//                await e.Channel.SendMessageDirectedAt(e.User.Id, $"the voting system is currently disabled.");
//                return;
//            }

//            //!vote ban24h <player name> reason <text>
//            //!votes
//            //!vote alpha yes
//            //!vote alpha no
//            var args = CommandHelper.ParseArgs(e, new { ServerKey = "", Ban = "", Ban24h = "", Unban = "", SetTimeOfDay = "", DestroyWildDinos = false, UpdateServer = false, RestartServer = false, Reason = "", Yes = false, No = false, Veto = false, History = false,
//                Alfa = false, Bravo = false, Charlie = false, Delta = false, Echo = false, Foxtrot = false, Golf = false, Hotel = false, India = false,
//                Juliett = false, Kilo = false, Lima = false, Mike = false, November = false, Oscar = false, Papa = false, Quebec = false, Romeo = false,
//                Sierra = false, Tango = false, Uniform = false, Victor = false, Whiskey = false, Xray = false, Yankee = false, Zulu = false
//            }, x =>
//                x.For(y => y.ServerKey, noPrefix: true)
//                .For(y => y.Ban, untilNextToken: true)
//                .For(y => y.Unban, untilNextToken: true)
//                .For(y => y.Ban24h, untilNextToken: true)
//                .For(y => y.Reason, untilNextToken: true)
//                .For(y => y.DestroyWildDinos, flag: true)
//                .For(y => y.UpdateServer, flag: true)
//                .For(y => y.RestartServer, flag: true)
//                .For(y => y.Yes, flag: true)
//                .For(y => y.No, flag: true)
//                .For(y => y.Veto, flag: true)
//                .For(y => y.History, flag: true)
//                .For(y => y.Alfa, flag: true)
//                .For(y => y.Bravo, flag: true)
//                .For(y => y.Charlie, flag: true)
//                .For(y => y.Delta, flag: true)
//                .For(y => y.Echo, flag: true)
//                .For(y => y.Foxtrot, flag: true)
//                .For(y => y.Golf, flag: true)
//                .For(y => y.Hotel, flag: true)
//                .For(y => y.India, flag: true)
//                .For(y => y.Juliett, flag: true)
//                .For(y => y.Kilo, flag: true)
//                .For(y => y.Lima, flag: true)
//                .For(y => y.Mike, flag: true)
//                .For(y => y.November, flag: true)
//                .For(y => y.Oscar, flag: true)
//                .For(y => y.Papa, flag: true)
//                .For(y => y.Quebec, flag: true)
//                .For(y => y.Romeo, flag: true)
//                .For(y => y.Sierra, flag: true)
//                .For(y => y.Tango, flag: true)
//                .For(y => y.Uniform, flag: true)
//                .For(y => y.Victor, flag: true)
//                .For(y => y.Whiskey, flag: true)
//                .For(y => y.Xray, flag: true)
//                .For(y => y.Yankee, flag: true)
//                .For(y => y.Zulu, flag: true));

//            var identifiers = args.Alfa || args.Bravo || args.Charlie || args.Delta || args.Echo || args.Foxtrot || args.Golf || args.Hotel || args.India
//                    || args.Juliett || args.Kilo || args.Lima || args.Mike || args.November || args.Oscar || args.Papa || args.Quebec || args.Romeo
//                    || args.Sierra || args.Tango || args.Uniform || args.Victor || args.Whiskey || args.Xray || args.Yankee || args.Zulu;

//            var identifierStrings = TypeDescriptor.GetProperties(args.GetType()).OfType<PropertyDescriptor>().Where(x => _identifierStrings.Contains(x.Name, StringComparer.OrdinalIgnoreCase) && x.PropertyType == typeof(bool) && (bool)x.GetValue(args)).Select(x => x.Name).ToArray();
//            var voteStrings = TypeDescriptor.GetProperties(args.GetType()).OfType<PropertyDescriptor>().Where(x => _voteStrings.Contains(x.Name, StringComparer.OrdinalIgnoreCase) && x.PropertyType == typeof(bool) && (bool)x.GetValue(args)).Select(x => x.Name).ToArray();

//            var isAdminOrDev = _discord.Servers.Any(x => x.Roles.Any(y => y != null && new[] { _config.AdminRoleName, _config.DeveloperRoleName }.Contains(y.Name, StringComparer.OrdinalIgnoreCase) == true && y.Members.Any(z => z.Id == e.User.Id)));
//            if (args == null || (identifiers && (voteStrings.Length == 0 || voteStrings.Length > 1 || identifierStrings.Length == 0 || identifierStrings.Length > 1))  //votes require an identifier and a Yes/No/Veto
//            )
//            {
//                await e.Channel.SendMessage(string.Join(Environment.NewLine, new string[] {
//                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
//                    !string.IsNullOrWhiteSpace(SyntaxHelp) ? $"Help me by following this syntax: **!{Name}** {SyntaxHelp}" : null }.Where(x => x != null)));
//                return;
//            }

//            var now = DateTime.Now;
//            var sb = new StringBuilder();

//            //this should match any type of vote
//            if (!string.IsNullOrWhiteSpace(args.Ban) || !string.IsNullOrWhiteSpace(args.Ban24h) || !string.IsNullOrWhiteSpace(args.Unban) || !string.IsNullOrWhiteSpace(args.SetTimeOfDay) || args.DestroyWildDinos || args.UpdateServer || args.RestartServer)
//            {
//                var serverContext = args?.ServerKey != null ? _contextManager.GetServer(args.ServerKey) : null;
//                if (serverContext == null)
//                {
//                    await e.Channel.SendMessageDirectedAt(e.User.Id, $"**Votes need to be prefixed with a server instance key.**");
//                    return;
//                }
//                if (!serverContext.IsInitialized)
//                {
//                    await e.Channel.SendMessage($"The server data is loading but is not ready yet...");
//                    return;
//                }

//                if (string.IsNullOrWhiteSpace(args.Reason))
//                {
//                    await e.Channel.SendMessageDirectedAt(e.User.Id, $"specifying a reason is mandatory when starting a vote.");
//                    return;

//                }

//                //is the user allowed to start a vote?
//                //level >= 50 && days registered >= 7

//                //is the username valid?

//                //is there already an ongoing vote for the same thing?
//                using (var db = _databaseContextFactory.Create())
//                {
//                    var user = db.Users.FirstOrDefault(x => x.DiscordId == (long)e.User.Id && !x.Unlinked);
//                    if (user == null)
//                    {
//                        await e.Channel.SendMessageDirectedAt(e.User.Id, $"this command can only be used after you link your Discord user with your Steam account using **!linksteam**.");
//                        return;
//                    }

//                    if (user.DisallowVoting)
//                    {
//                        await e.Channel.SendMessageDirectedAt(e.User.Id, $"you are not allowed to vote.");
//                        return;
//                    }

//                    if (!isAdminOrDev)
//                    {
//                        var player = serverContext.Players.FirstOrDefault(x => x.SteamId != null && x.SteamId.Equals(user.SteamId.ToString()));
//                        if (player == null)
//                        {
//                            await e.Channel.SendMessageDirectedAt(e.User.Id, $"we have no record of you playing in the last month which is a requirement for using this command.");
//                            return;
//                        }
                    
//                        if (player.CharacterLevel < 50)
//                        {
//                            await e.Channel.SendMessageDirectedAt(e.User.Id, $"you have to reach level 50 in order to initiate votes.");
//                            return;
//                        }

//                        var firstPlayed = user.Played?.Count > 0 ? user.Played?.Min(x => x.Date) : null;
//                        if (!firstPlayed.HasValue || (now - user.Played.Min(x => x.Date)).TotalDays < 7d)
//                        {
//                            await e.Channel.SendMessageDirectedAt(e.User.Id, $"you have to have played for atleast seven days in order to initiate votes.");
//                            return;
//                        }
//                    }

//                    var availableIdentifiers = _identifierStrings.Except(db.Votes.Where(x => x.Result == VoteResult.Undecided).Select(x => x.Identifier).ToArray(), StringComparer.OrdinalIgnoreCase).ToArray();
//                    var identifier = availableIdentifiers.Length > 0 ? availableIdentifiers.ElementAt(_rnd.Next(availableIdentifiers.Length)) : null;
//                    if (identifier == null)
//                    {
//                        await e.Channel.SendMessageDirectedAt(e.User.Id, $"there are too many active votes at the moment. Please try again later.");
//                        return;
//                    }

//                    InitiateVoteResult result = null;

//                    //handling of specific types
//                    if (!string.IsNullOrWhiteSpace(args.Ban))
//                    {
//                        result = await BanVoteHandler.Initiate(e.Channel, serverContext, _config, db, e.User.Id, identifier, now, args.Reason, args.Ban, 365 * 24 * 50);
//                    }
//                    else if (!string.IsNullOrWhiteSpace(args.Ban24h))
//                    {
//                        result = await BanVoteHandler.Initiate(e.Channel, serverContext, _config, db, e.User.Id, identifier, now, args.Reason, args.Ban24h, 24);
//                    }
//                    else if (!string.IsNullOrWhiteSpace(args.Unban))
//                    {
//                        result = await UnbanVoteHandler.Initiate(e.Channel, serverContext, _config, db, e.User.Id, identifier, now, args.Reason, args.Unban);
//                    }
//                    else if(!string.IsNullOrWhiteSpace(args.SetTimeOfDay))
//                    {
//                        result = await SetTimeOfDayVoteHandler.Initiate(e.Channel, serverContext, _config, db, e.User.Id, identifier, now, args.Reason, args.SetTimeOfDay);
//                    }
//                    else if (args.DestroyWildDinos)
//                    {
//                        result = await DestroyWildDinosVoteHandler.Initiate(e.Channel, serverContext, _config, db, e.User.Id, identifier, now, args.Reason);
//                    }
//                    else if (args.UpdateServer)
//                    {
//                        result = await UpdateServerVoteHandler.Initiate(e.Channel, serverContext, _config, db, e.User.Id, identifier, now, args.Reason);
//                    }
//                    else if (args.RestartServer)
//                    {
//                        result = await RestartServerVoteHandler.Initiate(e.Channel, serverContext, _config, db, e.User.Id, identifier, now, args.Reason);
//                    }
//                    else
//                    {
//                        //this should never happen
//                        throw new ApplicationException("The vote type is not handled in code...");
//                    }

//                    if (result == null || result.Vote == null) return;

//                    result.Vote.Votes.Add(new UserVote
//                    {
//                        InitiatedVote = true,
//                        Reason = args.Reason,
//                        VotedFor = true,
//                        User = user,
//                        UserId = user.Id,
//                        VoteType = isAdminOrDev ? UserVoteType.Admin : UserVoteType.Trusted,
//                        When = now
//                    });

//                    db.Votes.Add(result.Vote);
//                    db.SaveChanges();

//                    if (!string.IsNullOrWhiteSpace(result.MessageInitiator)) await e.Channel.SendMessageDirectedAt(e.User.Id, result.MessageInitiator);

//                    try
//                    {
//                        if (!string.IsNullOrWhiteSpace(result.MessageRcon)) await serverContext.Steam.SendRconCommand($"serverchat {result.MessageRcon.ReplaceRconSpecialChars()}");

//                        if (!string.IsNullOrWhiteSpace(result.MessageAnnouncement) && !string.IsNullOrWhiteSpace(_config.AnnouncementChannel))
//                        {
//                            var channels = _discord.Servers.Select(x => x.TextChannels.FirstOrDefault(y => _config.AnnouncementChannel.Equals(y.Name, StringComparison.OrdinalIgnoreCase))).Where(x => x != null);
//                            foreach (var channel in channels)
//                            {
//                                await channel.SendMessage(result.MessageAnnouncement);
//                            }
//                        }
//                    }
//                    catch { /*ignore all exceptions */ }

//                    serverContext.OnVoteInitiated(result.Vote);

//                    return;
//                }
//            }
//            else if (identifiers && (args.Yes || args.No || args.Veto))
//            {
//                if (!_context.IsInitialized)
//                {
//                    await e.Channel.SendMessage($"The server data is loading but is not ready yet...");
//                    return;
//                }

//                //does the identifier match an ongoing vote?

//                //have the user already voted?
//                using (var db = _databaseContextFactory.Create())
//                {
//                    var identifier = identifierStrings.First();
//                    var allvotes = db.Votes.ToArray();
//                    var votes = db.Votes.Where(x => x.Result == VoteResult.Undecided && x.Finished > now && x.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase)).ToArray();
//                    if (votes.Length == 0)
//                    {
//                        await e.Channel.SendMessageDirectedAt(e.User.Id, $"there is no active vote with the given identifier.");
//                        return;
//                    }
//                    if (votes.Length > 1)
//                    {
//                        await e.Channel.SendMessageDirectedAt(e.User.Id, $"there are multiple active votes with this identifier. This is a bug and the vote cannot be cast.");
//                        return;
//                    }
//                    var vote = votes.First();

//                    if (args.Veto && !isAdminOrDev)
//                    {
//                        await e.Channel.SendMessageDirectedAt(e.User.Id, $"you do not have this permission.");
//                        return;
//                    }

//                    var user = db.Users.FirstOrDefault(x => x.DiscordId == (long)e.User.Id && !x.Unlinked);
//                    if (user == null)
//                    {
//                        await e.Channel.SendMessageDirectedAt(e.User.Id, $"this command can only be used after you link your Discord user with your Steam account using **!linksteam**.");
//                        return;
//                    }

//                    if (user.DisallowVoting)
//                    {
//                        await e.Channel.SendMessageDirectedAt(e.User.Id, $"you are not allowed to vote.");
//                        return;
//                    }

//                    //todo: players are only considered for one server
//                    var player = _context.Players.FirstOrDefault(x => x.SteamId != null && x.SteamId.Equals(user.SteamId.ToString()));
//                    if (player == null)
//                    {
//                        await e.Channel.SendMessageDirectedAt(e.User.Id, $"we have no record of you playing in the last month which is a requirement for using this command.");
//                        return;
//                    }

//                    string message = null;
//                    UserVote uservote = null;
//                    if ((uservote = vote.Votes.FirstOrDefault(x => x.UserId == user.Id)) != null)
//                    {
//                        //await e.Channel.SendMessageDirectedAt(e.User.Id, $"you have already voted.");
//                        //return;

//                        uservote.Reason = args.Reason;
//                        uservote.VotedFor = args.Veto ? false : args.Yes;
//                        uservote.Vetoed = args.Veto;
//                        uservote.When = now;

//                        message = $"your vote has been changed!";
//                    }
//                    else
//                    {
//                        var firstPlayed = user.Played?.Count > 0 ? user.Played.Min(x => x.Date) : (DateTime?)null;

//                        vote.Votes.Add(new UserVote
//                        {
//                            InitiatedVote = false,
//                            Reason = args.Reason,
//                            VotedFor = args.Veto ? false : args.Yes,
//                            Vetoed = args.Veto,
//                            User = user,
//                            UserId = user.Id,
//                            VoteType = isAdminOrDev ? UserVoteType.Admin : !firstPlayed.HasValue || (now - user.Played.Min(x => x.Date)).TotalDays < 7d ? UserVoteType.NewMember : player.Level < 50 ? UserVoteType.Member : UserVoteType.Trusted,
//                            When = now
//                        });

//                        message = $"your vote has been recorded!";
//                    }
//                    db.SaveChanges();

//                    if (args.Veto)
//                    {
//                        var serverContext = _contextManager.GetServer(vote.ServerKey);
//                        if (serverContext != null)
//                        {
//                            serverContext.OnVoteResultForced(vote, VoteResult.Vetoed);
//                            message = $"you have vetoed this vote.";
//                        } else message = $"could not veto vote because there was no server key.";
//                    }

//                    await e.Channel.SendMessageDirectedAt(e.User.Id, message);
//                }
//            }
//            else
//            {
//                using (var db = _databaseContextFactory.Create())
//                {
//                    if (args.History)
//                    {
//                        var votes = db.Votes.OrderByDescending(x => x.Started).Where(x => x.Result != VoteResult.Undecided && System.Data.Entity.DbFunctions.DiffDays(now, x.Started) <= 7).ToArray();
//                        if (votes.Length == 0)
//                        {
//                            await e.Channel.SendMessageDirectedAt(e.User.Id, $"there appear to be no history of votes from the last seven days.");
//                            return;
//                        }

//                        sb.AppendLine("**Previous votes**");
//                        sb.AppendLine("--------------------");
//                        foreach (var vote in votes)
//                        {
//                            sb.AppendLine($"***{vote.Started:yyyy-MM-dd HH:mm}***");
//                            sb.AppendLine("**" + vote.ToString() + "**");
//                            sb.AppendLine(vote.Result == VoteResult.Passed ? ":white_check_mark: The vote passed" : vote.Result == VoteResult.Failed ? ":x: The vote failed" : vote.Result == VoteResult.Vetoed ? ":x: The vote was vetoed" : "Result is unknown").AppendLine();
//                        }
//                    }
//                    else
//                    {
//                        var votes = db.Votes.OrderByDescending(x => x.Started).Where(x => x.Result == VoteResult.Undecided).ToArray();
//                        if (votes.Length == 0)
//                        {
//                            await e.Channel.SendMessageDirectedAt(e.User.Id, $"there are no active votes at this time.");
//                            return;
//                        }

//                        sb.AppendLine("**Active Votes**");
//                        sb.AppendLine("--------------------");
//                        foreach (var vote in votes)
//                        {
//                            sb.AppendLine("**" + vote.ToString() + "**");
//                            sb.AppendLine($"To vote use the command: **!vote {vote.Identifier} yes**/**no**").AppendLine();
//                        }
//                    }
//                }
//            }

//            var msg = sb.ToString();
//            if (!string.IsNullOrWhiteSpace(msg)) await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
//        }
//    }
//}
