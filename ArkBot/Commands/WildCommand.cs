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
using System.Globalization;

namespace ArkBot.Commands
{
    public class WildCommand : ICommand
    {
        public string Name => "wild";
        public string[] Aliases => new[] { "wilddinos" };
        public string Description => "Status information for wild dinos";
        public string SyntaxHelp => "[***compareto <yyyy-mm-dd hh:mm:ss (date in the past)>***]";
        public string[] UsageExamples => new[]
        {
            ": Compare current state to previous adjacent state",
            $"**compareto {DateTime.Today.AddDays(-1).Date.AddHours(12)}**: Compare current state to the state ***yesterday at noon***"
        };

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        private IArkContext _context;
        private IConstants _constants;
        private DatabaseContextFactory<IEfDatabaseContext> _databaseContextFactory;

        public WildCommand(IArkContext context, IConstants constants, DatabaseContextFactory<IEfDatabaseContext> databaseContextFactory)
        {
            _context = context;
            _constants = constants;
            _databaseContextFactory = databaseContextFactory;
        }

        public void Register(CommandBuilder command)
        {
            command.Parameter("optional", ParameterType.Multiple);
        }

        public async Task Run(CommandEventArgs e)
        {
            using (var context = _databaseContextFactory.Create())
            {
                var args = CommandHelper.ParseArgs(e, new { CompareTo = DateTime.MinValue }, x =>
                    x.For(y => y.CompareTo, untilNextToken: true, formatProvider: CultureInfo.CurrentCulture));
                if (args == null || ((args.CompareTo == DateTime.MinValue || args.CompareTo > _context.LastUpdate) && e.Args.Length > 0))
                {
                    await e.Channel.SendMessage(string.Join(Environment.NewLine, new string[] {
                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
                    !string.IsNullOrWhiteSpace(SyntaxHelp) ? $"Help me by following this syntax: **!{Name}** {SyntaxHelp}" : null }.Where(x => x != null)));
                    return;
                }

                var sb = new StringBuilder();


                Database.Model.WildCreatureLog log = null;
                if (args.CompareTo == DateTime.MinValue) log = context.WildCreatureLogs.OrderByDescending(x => x.When).Skip(1).FirstOrDefault();
                else
                {
                    log = context.WildCreatureLogs.Where(x => x.When <= args.CompareTo && x.When != _context.LastUpdate).OrderByDescending(x => x.When).FirstOrDefault();
                    if (log == null)
                    {
                        await e.Channel.SendMessage($"**The specified date predates any logs we have! :(**");
                        return;
                    }
                }

                var wild = _context.Wild?.GroupBy(x => x.SpeciesClass).Select(x =>
                {
                    var ids = x.Select(y => y.Id).ToArray();
                    var logIds = log?.Entries?.FirstOrDefault(y => y.Key.Equals(x.Key, StringComparison.OrdinalIgnoreCase))?.Ids;
                //var logIntersection = log != null ? log.Entries.FirstOrDefault(y => y.Key.Equals(x.Key, StringComparison.OrdinalIgnoreCase))?.Ids?.Intersect(ids) : null;
                var count = x.Count();
                    var same = logIds != null ? logIds.Intersect(ids).Count() : 0;
                    return new
                    {
                        Key = _context.SpeciesAliases.GetAliases(x.Key)?.FirstOrDefault() ?? x.Key,
                        Now = count,
                        Prev = logIds != null ? logIds.Length : 0,
                        ChangePercent = logIds != null ? logIds.Length > 0 ? Math.Round(((count / (double)logIds.Length) - 1) * 100) : 0d : 0d,
                        Additions = logIds != null ? ids.Except(logIds).Count() : 0,
                        Deletions = logIds != null ? -logIds.Except(ids).Count() : 0,
                        Same = same,
                        SamePercentage = logIds != null ? count > 0 ? (same / (double)count) * 100 : 0d : 0d,
                        Ids = ids
                    };
                }).ToArray();
                var results = wild.Where(x => x.Now >= 25).OrderByDescending(x => x.Now).ToArray();
                if (results.Length > 0)
                {
                    var nextUpdate = _context.ApproxTimeUntilNextUpdate;
                    var nextUpdateTmp = nextUpdate?.ToStringCustom();
                    var nextUpdateString = (nextUpdate.HasValue ? (!string.IsNullOrWhiteSpace(nextUpdateTmp) ? $", next update in ~{nextUpdateTmp}" : ", waiting for new update ...") : "");
                    var lastUpdate = _context.LastUpdate;
                    var lastUpdateString = lastUpdate.ToStringWithRelativeDay();

                    sb.AppendLine($"**Wild creatures status (25 or above)" + (args.CompareTo == DateTime.MinValue || log == null ? "" : ", compared to " + log.When.ToStringWithRelativeDay()) + $"** (updated {lastUpdateString}{nextUpdateString})");
                    sb.AppendLine("```");
                    sb.AppendLine(FixedWidthTableHelper.ToString(results, x => x
                        .For(y => y.Key, "Species")
                        .For(y => y.Now, "Now", 1, "N0", total: true)
                        .For(y => y.Prev, "Prev", 1, "N0", fordefault: "", total: true)
                        .For(y => y.ChangePercent, "% Change", 1, "+#'%';-#'%';0'%'", fordefault: "", aggregate: (r) => { var t = (double)r.Sum(z => z.Prev); return t > 0 ? ((r.Sum(z => z.Now) / t) - 1) * 100 : 0d; })
                        .For(y => y.Additions, "Additions", 1, "+#,#;-#,#;0", fordefault: "", total: true)
                        .For(y => y.Deletions, "Deletions", 1, "+#,#;-#,#;0", fordefault: "", total: true)
                        .For(y => y.Same, hide: true)
                        .For(y => y.SamePercentage, "% Same", 1, "#'%';-#'%';0'%'", fordefault: "", aggregate: (r) => { var t = (double)r.Sum(z => z.Now); return t > 0 ? r.Sum(z => z.Same) / t * 100 : 0d; })
                        .For(y => y.Ids, hide: true)));
                    sb.AppendLine("```");
                }

                await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
            }
        }
    }
}
