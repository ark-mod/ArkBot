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
using ArkBot.Database.Model;
using ArkBot.Data;

namespace ArkBot.Commands
{
    public class MyDinoHistoryCommand : ICommand
    {
        public string Name => "mydinos";
        public string[] Aliases => null;
        public string Description => "List your current-, dead-, missing-, uploaded- or unavailable dinos";
        public string SyntaxHelp => "[<option (***dead/missing/uploaded/unavailable***)>] [***skip <number>***]";
        public string[] UsageExamples => new []
        {
            ": Returns a list of your current dinos",
            "**dead**: Returns a list of your confirmed ***dead*** dinos",
            "**missing**: Returns a list of your dinos that for unknown reasons are ***missing***",
            "**uploaded**: Returns a list of your ***uploaded*** dinos",
            "**unavailable**: Returns a list of all your unavailable dinos (dead, missing and uploaded)",
        };

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        private IArkContext _context;
        private DatabaseContextFactory<IEfDatabaseContext> _databaseContextFactory;

        public MyDinoHistoryCommand(IArkContext context, DatabaseContextFactory<IEfDatabaseContext> databaseContextFactory)
        {
            _context = context;
            _databaseContextFactory = databaseContextFactory;
        }

        public void Register(CommandBuilder command)
        {
            command.Parameter("optional", ParameterType.Multiple);
        }

        public async Task Run(CommandEventArgs e)
        {
            var take = 25;

            var args = CommandHelper.ParseArgs(e, new { Dead = false, Missing = false, Uploaded = false, Unavailable = false, Skip = 0 }, x =>
                x.For(y => y.Dead, flag: true)
                .For(y => y.Missing, flag: true)
                .For(y => y.Uploaded, flag: true)
                .For(y => y.Unavailable, flag: true)
                .For(y => y.Skip, defaultValue: 0));
            if (args == null || args.Skip < 0)
            {
                await e.Channel.SendMessage(string.Join(Environment.NewLine, new string[] {
                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
                    !string.IsNullOrWhiteSpace(SyntaxHelp) ? $"Help me by following this syntax: **!{Name}** {SyntaxHelp}" : null }.Where(x => x != null)));
                return;
            }

            var player = await CommandHelper.GetCurrentPlayerOrSendErrorMessage(e, _databaseContextFactory, _context);
            if (player == null) return;

            if(player.TribeId == null && args.Dead)
            {
                //dead creatures can only be found using tribe log
                await e.Channel.SendMessage($"<@{e.User.Id}>, You have to be in a tribe to find dead dinos!");
                return;
            }

            var nextUpdate = _context.ApproxTimeUntilNextUpdate;
            var nextUpdateTmp = nextUpdate?.ToStringCustom();
            var nextUpdateString = (nextUpdate.HasValue ? (!string.IsNullOrWhiteSpace(nextUpdateTmp) ? $", next update in ~{nextUpdateTmp}" : ", waiting for new update ...") : "");
            var lastUpdate = _context.LastUpdate;
            var lastUpdateString = lastUpdate.ToStringWithRelativeDay();

            if (!(args.Dead || args.Missing || args.Unavailable))
            {
                IEnumerable<Creature> filtered = null;

                if (args.Uploaded) filtered = _context.Cluster?.Creatures.Where(x => (x.PlayerId.HasValue && x.PlayerId.Value == player.Id) || (x.Team.HasValue && x.Team.Value == player.TribeId));
                else filtered = _context.Creatures?.Where(x => (x.PlayerId.HasValue && x.PlayerId.Value == player.Id) || (x.Team.HasValue && x.Team.Value == player.TribeId));

                var dinos = filtered?.OrderByDescending(x => x.FullLevel ?? x.BaseLevel).ThenByDescending(x => x.Experience ?? decimal.MinValue).Skip(args.Skip).Take(take).ToArray();
                var count = filtered?.Count() ?? 0;

                if (args.Skip > 0 && args.Skip >= count)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, you asked me to skip more dinos than you have!");
                    return;
                }
                else if (dinos == null || dinos.Length <= 0)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, it appears you do not have any" + (args.Uploaded ? " uploaded" : "") + " dinos... :(");
                    return;
                }

                var sb = new StringBuilder();
                sb.Append($"**My records show you have {count:N0}" + (args.Uploaded ? " uploaded" : "") + " dinos");
                if (count > 10) sb.Append($" (showing {args.Skip + 1}-{args.Skip + dinos.Length})");
                sb.AppendLine($"** (updated {lastUpdateString}{nextUpdateString})");
                foreach (var x in dinos)
                {
                    sb.AppendLine($"● {(!string.IsNullOrWhiteSpace(x.Name) ? $"**{x.Name}**, ***{x.SpeciesName}***" : $"**{x.SpeciesName}**")} (lvl ***{x.FullLevel ?? x.BaseLevel}***)");
                }

                await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
            }
            else
            {
                using (var db = _databaseContextFactory.Create())
                {
                    var filtered = db.TamedCreatureLogEntries.Where(x => (x.PlayerId.HasValue && x.PlayerId.Value == player.Id) || (x.Team.HasValue && x.Team.Value == player.TribeId));

                    if (args.Unavailable) filtered = filtered.Where(x => x.IsUnavailable);
                    else if (args.Dead) filtered = filtered.Where(x => x.IsConfirmedDead);
                    else if (args.Missing) filtered = filtered.Where(x => x.IsUnavailable && x.IsConfirmedDead == false && x.IsInCluster == false);
                    else if (args.Uploaded) filtered = filtered.Where(x => x.IsInCluster);

                    var dinos = filtered?.OrderByDescending(x => x.LastSeen).ThenBy(x => x.Name).Skip(args.Skip).Take(take).ToArray();
                    var count = filtered?.Count() ?? 0;

                    if (args.Skip > 0 && args.Skip >= count)
                    {
                        await e.Channel.SendMessage($"<@{e.User.Id}>, you asked me to skip more dinos than I could find!");
                        return;
                    }
                    else if (dinos == null || dinos.Length <= 0)
                    {
                        await e.Channel.SendMessage($"<@{e.User.Id}>, sadly I could not find any of the dinos you were looking for... :(");
                        return;
                    }
                    
                    var type = args.Unavailable ? "unavailable" : args.Dead ? "dead" : args.Missing ? "missing" : "";

                    var sb = new StringBuilder();
                    sb.Append($"**My records show you have {count:N0} {type} dinos");
                    if (count > 10) sb.Append($" (showing {args.Skip + 1}-{args.Skip + dinos.Length})");
                    sb.AppendLine($"** (updated {lastUpdateString}{nextUpdateString})");
                    foreach (var x in dinos)
                    {
                        var speciesName = _context.SpeciesAliases.GetAliases(x.SpeciesClass)?.FirstOrDefault() ?? x.SpeciesClass;
                        sb.Append($"● {(!string.IsNullOrWhiteSpace(x.Name) ? $"**{x.Name}**, ***{speciesName}***" : $"**{speciesName}**")} (lvl ***{x.FullLevel ?? x.BaseLevel}***)");
                        if (x.IsConfirmedDead)
                        {
                            var relatedLogEntry = x.RelatedLogEntries != null ? x.RelatedLogEntries.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault() : null;
                            var log = relatedLogEntry != null ? TameWasKilledTribeLog.FromLog(relatedLogEntry) : null;
                            if (log == null) sb.Append($" was killed/died ***{x.LastSeen.ToStringWithRelativeDay(formatTime: "'~'HH:mm")}***");
                            else if (log.KilledBy == null || log.KilledByLevel == null) sb.Append($" was killed/died ***{x.LastSeen.ToStringWithRelativeDay(formatTime:"'~'HH:mm")}***");
                            else sb.Append($" was killed by ***{log.KilledBy}*** (lvl ***{log.KilledByLevel}***) ***{x.LastSeen.ToStringWithRelativeDay(formatTime: "'~'HH:mm")}***");
                        }
                        else if (x.IsInCluster) sb.Append(" **[Uploaded]**");
                        else if (x.IsUnavailable && x.IsConfirmedDead == false && x.IsInCluster == false)
                        {
                            sb.Append($" was last seen ***{x.LastSeen.ToStringWithRelativeDay()}*** at " + Invariant($"***{x.Latitude:N1}***, ***{x.Longitude:N1}***, ***altitude {_context.GetElevationAsText(x.Z)}***") + (x.ApproxFoodPercentage.HasValue ? $" with ***{x.ApproxFoodPercentage.Value:N0}% food remaining***" : "") + " and is now missing");
                        }
                        sb.AppendLine();
                    }

                    await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
                }
            }
        }
    }
}
