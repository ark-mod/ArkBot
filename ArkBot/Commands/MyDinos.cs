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
//using ArkBot.Database;
//using ArkBot.Database.Model;
//using ArkBot.Data;
//using Discord;
//using ArkBot.Ark;

//namespace ArkBot.Commands
//{
//    public class MyDinoHistoryCommand : ICommand
//    {
//        public string Name => "mydinos";
//        public string[] Aliases => null;
//        //public string Description => "List your current-, dead-, missing-, uploaded- or unavailable dinos";
//        public string Description => "List your current- or uploaded dinos";
//        //public string SyntaxHelp => "[<option (***dead/missing/uploaded/unavailable***)>] [***skip <number>***]";
//        public string SyntaxHelp => "<***server key***> [<option (***uploaded***)>] [***skip <number>***]";
//        public string[] UsageExamples => new []
//        {
//            "**<server key>**: Returns a list of your current dinos",
//            //"**dead**: Returns a list of your confirmed ***dead*** dinos",
//            //"**missing**: Returns a list of your dinos that for unknown reasons are ***missing***",
//            "**<server key>** **uploaded**: Returns a list of your ***uploaded*** dinos",
//            //"**unavailable**: Returns a list of all your unavailable dinos (dead, missing and uploaded)",
//        };

//        public bool DebugOnly => false;
//        public bool HideFromCommandList => false;

//        private EfDatabaseContextFactory _databaseContextFactory;
//        private ArkContextManager _contextManager;

//        public MyDinoHistoryCommand(
//            EfDatabaseContextFactory databaseContextFactory,
//            ArkContextManager contextManager)
//        {
//            _databaseContextFactory = databaseContextFactory;
//            _contextManager = contextManager;
//        }

//        public void Register(CommandBuilder command)
//        {
//            command.Parameter("optional", ParameterType.Multiple);
//        }

//        public void Init(DiscordClient client) { }

//        public async Task Run(CommandEventArgs e)
//        {
//            var take = 25;

//            //var args = CommandHelper.ParseArgs(e, new { Dead = false, Missing = false, Uploaded = false, Unavailable = false, Skip = 0 }, x =>
//            //    x.For(y => y.Dead, flag: true)
//            //    .For(y => y.Missing, flag: true)
//            //    .For(y => y.Uploaded, flag: true)
//            //    .For(y => y.Unavailable, flag: true)
//            //    .For(y => y.Skip, defaultValue: 0));

//            var args = CommandHelper.ParseArgs(e, new { ServerKey = "", Uploaded = false, Skip = 0 }, x =>
//                x.For(y => y.ServerKey, noPrefix: true, isRequired: true)
//                .For(y => y.Uploaded, flag: true)
//                .For(y => y.Skip, defaultValue: 0));

//            if (args == null || args.Skip < 0)
//            {
//                await e.Channel.SendMessage(string.Join(Environment.NewLine, new string[] {
//                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
//                    !string.IsNullOrWhiteSpace(SyntaxHelp) ? $"Help me by following this syntax: **!{Name}** {SyntaxHelp}" : null }.Where(x => x != null)));
//                return;
//            }

//            var serverContext = _contextManager.GetServer(args.ServerKey);
//            if (serverContext == null)
//            {
//                await e.Channel.SendMessage($"**Specified server instance key is not valid.**");
//                return;
//            }

//            if (!serverContext.IsInitialized)
//            {
//                await e.Channel.SendMessage($"**The data is loading but is not ready yet...**");
//                return;
//            }

//            var player = await CommandHelper.GetCurrentPlayerOrSendErrorMessage(e, _databaseContextFactory, serverContext);
//            if (player == null) return;

//            //if(player.TribeId == null && args.Dead)
//            //{
//            //    //dead creatures can only be found using tribe log
//            //    await e.Channel.SendMessage($"<@{e.User.Id}>, You have to be in a tribe to find dead dinos!");
//            //    return;
//            //}

//            var nextUpdate = serverContext.ApproxTimeUntilNextUpdate;
//            var nextUpdateTmp = nextUpdate?.ToStringCustom();
//            var nextUpdateString = (nextUpdate.HasValue ? (!string.IsNullOrWhiteSpace(nextUpdateTmp) ? $", next update in ~{nextUpdateTmp}" : ", waiting for new update ...") : "");
//            var lastUpdate = serverContext.LastUpdate;
//            var lastUpdateString = lastUpdate.ToStringWithRelativeDay();

//            //if (!(args.Dead || args.Missing || args.Unavailable))
//            //{
//                IEnumerable<ArkSavegameToolkitNet.Domain.ArkTamedCreature> filtered = null;

//                if (args.Uploaded) filtered = serverContext.CloudCreatures?.Where(x => x.TargetingTeam == player.Id || (x.OwningPlayerId.HasValue && x.OwningPlayerId == player.Id));
//                else filtered = serverContext.NoRafts?.Where(x => x.TargetingTeam == player.Id || (x.OwningPlayerId.HasValue && x.OwningPlayerId == player.Id));

//                var dinos = filtered?.OrderByDescending(x => x.Level).ThenByDescending(x => x.ExperiencePoints ?? float.MinValue).Skip(args.Skip).Take(take).ToArray();
//                var count = filtered?.Count() ?? 0;

//                if (args.Skip > 0 && args.Skip >= count)
//                {
//                    await e.Channel.SendMessage($"<@{e.User.Id}>, you asked me to skip more dinos than you have!");
//                    return;
//                }
//                else if (dinos == null || dinos.Length <= 0)
//                {
//                    await e.Channel.SendMessage($"<@{e.User.Id}>, it appears you do not have any" + (args.Uploaded ? " uploaded" : "") + " dinos... :(");
//                    return;
//                }

//                var sb = new StringBuilder();
//                sb.Append($"**My records show you have {count:N0}" + (args.Uploaded ? " uploaded" : "") + " dinos");
//                if (count > 10) sb.Append($" (showing {args.Skip + 1}-{args.Skip + dinos.Length})");
//                sb.AppendLine($"** (updated {lastUpdateString}{nextUpdateString})");

//                //todo: cluster does not appear to have species name which makes it a bit hard to present which dinos are in the cluster
//                //Dictionary<long, string> species = new Dictionary<long, string>();
//                //if (args.Uploaded)
//                //{
//                //    var ids = dinos.Where(x => string.IsNullOrWhiteSpace(x.SpeciesClass)).Select(x => x.Id).ToArray();
//                //    if (ids.Length > 0)
//                //    {
//                //        using (var db = _databaseContextFactory.Create())
//                //        {
//                //            foreach (var item in db.TamedCreatureLogEntries.Where(x => ids.Contains(x.Id)).Select(x => new { id = x.Id, species = x.SpeciesClass }).ToArray())
//                //            {
//                //                species.Add(item.id, ArkSpeciesAliases.Instance.GetAliases(item.species)?.FirstOrDefault() ?? item.species);
//                //            }
//                //        }
//                //    }
//                //}

//                foreach (var x in dinos)
//                {
//                    var aliases = ArkSpeciesAliases.Instance.GetAliases(x.ClassName);
//                    var species = aliases?.FirstOrDefault() ?? x.ClassName;
//                    //todo: cluster does not appear to have species name which makes it a bit hard to present which dinos are in the cluster
//                    if (args.Uploaded)
//                    {
//                        var primary = x.Name;
//                        var secondary = species;
//                        if (string.IsNullOrWhiteSpace(primary))
//                        {
//                            primary = secondary ?? x.Id.ToString();
//                            secondary = null;
//                        }
//                        sb.AppendLine($"● **{primary}**" + (secondary != null ? $", ***{secondary}***" : "") + $" (lvl ***{x.Level}***)");
//                    }
//                    else
//                    {
//                        sb.AppendLine($"● {(!string.IsNullOrWhiteSpace(x.Name) ? $"**{x.Name}**, ***{species}***" : $"**{species}**")} (lvl ***{x.Level}***)");
//                    }
//                }

//                await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
//            //}
//            //else
//            //{
//            //    using (var db = _databaseContextFactory.Create())
//            //    {
//            //        var filtered = db.TamedCreatureLogEntries.Where(x => ((x.PlayerId.HasValue && x.PlayerId.Value == player.Id) || (x.Team.HasValue && x.Team.Value == player.TribeId)) && !x.SpeciesClass.Equals("Raft_BP_C", StringComparison.OrdinalIgnoreCase));

//            //        if (args.Unavailable) filtered = filtered.Where(x => x.IsUnavailable);
//            //        else if (args.Dead) filtered = filtered.Where(x => x.IsConfirmedDead);
//            //        else if (args.Missing) filtered = filtered.Where(x => x.IsUnavailable && x.IsConfirmedDead == false && x.IsInCluster == false);
//            //        else if (args.Uploaded) filtered = filtered.Where(x => x.IsInCluster);

//            //        var dinos = filtered?.OrderByDescending(x => x.LastSeen).ThenBy(x => x.Name).Skip(args.Skip).Take(take).ToArray();
//            //        var count = filtered?.Count() ?? 0;

//            //        if (args.Skip > 0 && args.Skip >= count)
//            //        {
//            //            await e.Channel.SendMessage($"<@{e.User.Id}>, you asked me to skip more dinos than I could find!");
//            //            return;
//            //        }
//            //        else if (dinos == null || dinos.Length <= 0)
//            //        {
//            //            await e.Channel.SendMessage($"<@{e.User.Id}>, sadly I could not find any of the dinos you were looking for... :(");
//            //            return;
//            //        }
                    
//            //        var type = args.Unavailable ? "unavailable" : args.Dead ? "dead" : args.Missing ? "missing" : "";

//            //        var sb = new StringBuilder();
//            //        sb.Append($"**My records show you have {count:N0} {type} dinos");
//            //        if (count > 10) sb.Append($" (showing {args.Skip + 1}-{args.Skip + dinos.Length})");
//            //        sb.AppendLine($"** (updated {lastUpdateString}{nextUpdateString})");
//            //        foreach (var x in dinos)
//            //        {
//            //            var speciesName = ArkSpeciesAliases.Instance.GetAliases(x.SpeciesClass)?.FirstOrDefault() ?? x.SpeciesClass;
//            //            sb.Append($"● {(!string.IsNullOrWhiteSpace(x.Name) ? $"**{x.Name}**, ***{speciesName}***" : $"**{speciesName}**")} (lvl ***{x.FullLevel ?? x.BaseLevel}***)");
//            //            if (x.IsConfirmedDead)
//            //            {
//            //                var relatedLogEntry = x.RelatedLogEntries != null ? x.RelatedLogEntries.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() : null;
//            //                var log = relatedLogEntry != null ? TameWasKilledTribeLog.FromLog(relatedLogEntry) : null;
//            //                if (log == null) sb.Append($" was killed/died ***{x.LastSeen.ToStringWithRelativeDay(formatTime: "'~'HH:mm")}***");
//            //                else if (log.KilledBy == null || log.KilledByLevel == null) sb.Append($" was killed/died ***{x.LastSeen.ToStringWithRelativeDay(formatTime:"'~'HH:mm")}***");
//            //                else sb.Append($" was killed by ***{log.KilledBy}*** (lvl ***{log.KilledByLevel}***) ***{x.LastSeen.ToStringWithRelativeDay(formatTime: "'~'HH:mm")}***");
//            //            }
//            //            else if (x.IsInCluster) sb.Append(" **[Uploaded]**");
//            //            else if (x.IsUnavailable && x.IsConfirmedDead == false && x.IsInCluster == false)
//            //            {
//            //                var stats = new[] {
//            //                    (x.ApproxHealthPercentage.HasValue ? $"***{(x.ApproxHealthPercentage.Value * 100):N0}% health" : null),
//            //                    (x.ApproxFoodPercentage.HasValue ? $"***{(x.ApproxFoodPercentage.Value * 100):N0}% food" : null)
//            //                }.Where(y => y != null).ToArray().Join((n, l) => n == l ? "-*** and " : "-***, ");
//            //                sb.Append($" was last seen ***{x.LastSeen.ToStringWithRelativeDay()}*** at " + Invariant($"***{x.Latitude:N1}***, ***{x.Longitude:N1}***, ***altitude {_context.GetElevationAsText(x.Z)}***") + (!string.IsNullOrEmpty(stats) ? $" with {stats} remaining***" : "") + " and is now missing");
//            //            }
//            //            sb.AppendLine();
//            //        }

//            //        await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
//            //    }
//            //}
//        }
//    }
//}
