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
//using System.Globalization;
//using ArkBot.Data;

//namespace ArkBot.Commands
//{
//    public class WildCommand : ICommand
//    {
//        public string Name => "wild";
//        public string[] Aliases => new[] { "wilddinos" };
//        public string Description => "Status information for wild dinos";
//        public string SyntaxHelp => "[***compareto <yyyy-mm-dd hh:mm:ss (date in the past)>***]";
//        public string[] UsageExamples => new[]
//        {
//            ": Compare current state to previous adjacent state",
//            $"**compareto {DateTime.Today.AddDays(-1).Date.AddHours(12)}**: Compare current state to the state ***yesterday at noon***"
//        };

//        public bool DebugOnly => false;
//        public bool HideFromCommandList => true;

//        private IArkContext _context;
//        private IConstants _constants;
//        private EfDatabaseContextFactory _databaseContextFactory;
//        private IConfig _config;

//        public WildCommand(IArkContext context, IConstants constants, EfDatabaseContextFactory databaseContextFactory, IConfig config)
//        {
//            _context = context;
//            _constants = constants;
//            _databaseContextFactory = databaseContextFactory;
//            _config = config;
//        }

//        public void Register(CommandBuilder command)
//        {
//            command.AddCheck((a, b, c) => c.Client.Servers.Any(x => x.Roles.Any(y => y != null && (y.Name.Equals(_config.DeveloperRoleName) || y.Name.Equals(_config.AdminRoleName)) && y.Members.Any(z => z.Id == b.Id))), null)
//                .Parameter("optional", ParameterType.Multiple)
//                .Hide();
//        }

//        public void Init(Discord.DiscordClient client) { }

//        public async Task Run(CommandEventArgs e)
//        {
//            if (!e.Channel.IsPrivate) return;

//            using (var context = _databaseContextFactory.Create())
//            {
//                var args = CommandHelper.ParseArgs(e, new { CompareTo = DateTime.MinValue, Map = "", NewMap = false }, x =>
//                    x.For(y => y.CompareTo, untilNextToken: true, formatProvider: CultureInfo.CurrentCulture)
//                    .For(y => y.Map, untilNextToken: true)
//                    .For(y => y.NewMap, flag: true));
//                if (args == null || (((args.CompareTo == DateTime.MinValue || args.CompareTo > _context.LastUpdate) && (string.IsNullOrWhiteSpace(args.Map))) && e.Args.Length > 0))
//                {
//                    await e.Channel.SendMessage(string.Join(Environment.NewLine, new string[] {
//                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
//                    !string.IsNullOrWhiteSpace(SyntaxHelp) ? $"Help me by following this syntax: **!{Name}** {SyntaxHelp}" : null }.Where(x => x != null)));
//                    return;
//                }

//                var sb = new StringBuilder();

//                if (!string.IsNullOrWhiteSpace(args.Map))
//                {
//                    //annotated map of current species spread
//                    var aliases = ArkSpeciesAliases.Instance.GetAliases(args.Map);
//                    var speciesNames = aliases ?? new[] { args.Map };

//                    var matches = _context.Wild?.Where(x => speciesNames != null && x.SpeciesClass != null && speciesNames.Contains(x.SpeciesClass, StringComparer.OrdinalIgnoreCase))
//                        .Select(x => new { lat = x.Latitude, lng = x.Longitude }).ToArray();
//                    if (matches == null || matches.Length < 1)
//                    {
//                        sb.AppendLine($"**No matching wild creatures found!**");
//                        if (aliases == null && _context.Wild != null && _context.ArkSpeciesStatsData?.SpeciesStats != null)
//                        {
//                            var sequence = args.Map.ToLower().ToCharArray();
//                            var similarity = ArkSpeciesAliases.Instance.Aliases.Select(x =>
//                            {
//                                var s = x.Select(y => new { key = y, s = StatisticsHelper.CompareToCharacterSequence(y, sequence) }).OrderByDescending(y => y.s).FirstOrDefault();
//                                return new { key = s.key, primary = x.FirstOrDefault(), all = x, val = s.s /*s >= 0 ? s : 0*/ };
//                            }).ToArray();
//                            var possible = StatisticsHelper.FilterUsingStandardDeviation(similarity, x => x.val, (dist, sd) => dist >= sd * 1.5, false);
//                            if (possible != null && possible.Length > 0)
//                            {
//                                var distances = possible.Select((x, i) => new { key = x.key, primary = x.primary, index = i, similarity = x.val, result = args.Map.FindLowestLevenshteinWordDistanceInString(x.key) })
//                                    .Where(x => x.result != null)
//                                    .OrderBy(x => x.result.Item2).ThenBy(x => x.similarity).ToArray();
//                                var best = StatisticsHelper.FilterUsingStandardDeviation(distances, x => x.result.Item2, (dist, sd) => dist <= sd, false);

//                                var suggestions = best.Select(x => $"***\"{x.primary}\"***").ToArray().Join((n, l) => n == l ? " *or* " : "\u200B*,* ");
//                                sb.AppendLine($"*Did you perhaps mean* {suggestions}\u200B*?*"); //\u200B
//                            }
//                        }
//                    }
//                    else
//                    {
//                        await CommandHelper.SendAnnotatedMap(e.Channel, matches.Select(x => new PointF((float)x.lng, (float)x.lat)).ToArray(), _config.TempFileOutputDirPath, args.NewMap ? 5f : 2f, Brushes.Magenta, template: args.NewMap ? MapTemplate.Vectorized : MapTemplate.Sketch);
//                    }
//                }
//                else
//                {
//                    Database.Model.WildCreatureLog log = null;
//                    if (args.CompareTo == DateTime.MinValue) log = context.WildCreatureLogs.OrderByDescending(x => x.When).Skip(1).FirstOrDefault();
//                    else
//                    {
//                        log = context.WildCreatureLogs.Where(x => x.When <= args.CompareTo && x.When != _context.LastUpdate).OrderByDescending(x => x.When).FirstOrDefault();
//                        if (log == null)
//                        {
//                            await e.Channel.SendMessage($"**The specified date predates any logs we have! :(**");
//                            return;
//                        }
//                    }
//                    var noPrev = log == null || log.Entries == null || log.Entries.Count <= 0;

//                    var wild = _context.Wild?.GroupBy(x => x.SpeciesClass).Select(x =>
//                    {
//                        var ids = x.Select(y => y.Id).ToArray();
//                        var logIds = log?.Entries?.FirstOrDefault(y => y.Key.Equals(x.Key, StringComparison.OrdinalIgnoreCase))?.Ids;
//                        //var logIntersection = log != null ? log.Entries.FirstOrDefault(y => y.Key.Equals(x.Key, StringComparison.OrdinalIgnoreCase))?.Ids?.Intersect(ids) : null;
//                        var count = x.Count();
//                        var same = logIds != null ? logIds.Intersect(ids).Count() : 0;
//                        return new
//                        {
//                            Key = ArkSpeciesAliases.Instance.GetAliases(x.Key)?.FirstOrDefault() ?? x.Key,
//                            Now = count,
//                            Prev = logIds != null ? logIds.Length : 0,
//                            ChangePercent = logIds != null ? logIds.Length > 0 ? Math.Round(((count / (double)logIds.Length) - 1) * 100) : 0d : 0d,
//                            Additions = logIds != null ? ids.Except(logIds).Count() : 0,
//                            Deletions = logIds != null ? -logIds.Except(ids).Count() : 0,
//                            Same = same,
//                            SamePercentage = logIds != null ? count > 0 ? (same / (double)count) * 100 : 0d : 0d,
//                            Ids = ids
//                        };
//                    }).ToArray();
//                    var results = wild.OrderByDescending(x => x.Now).ToArray();
//                    if (results.Length > 0)
//                    {
//                        var nextUpdate = _context.ApproxTimeUntilNextUpdate;
//                        var nextUpdateTmp = nextUpdate?.ToStringCustom();
//                        var nextUpdateString = (nextUpdate.HasValue ? (!string.IsNullOrWhiteSpace(nextUpdateTmp) ? $", next update in ~{nextUpdateTmp}" : ", waiting for new update ...") : "");
//                        var lastUpdate = _context.LastUpdate;
//                        var lastUpdateString = lastUpdate.ToStringWithRelativeDay();

//                        sb.AppendLine($"**Wild creatures status" + (args.CompareTo == DateTime.MinValue || log == null ? "" : ", compared to " + log.When.ToStringWithRelativeDay()) + $"** (updated {lastUpdateString}{nextUpdateString})");
//                        sb.AppendLine("```");
//                        sb.AppendLine(FixedWidthTableHelper.ToString(results, x => x
//                            .For(y => y.Key, "Species")
//                            .For(y => y.Now, "Now", 1, "N0", total: true)
//                            .For(y => y.Prev, "Prev", 1, "N0", fordefault: "", total: true, hide: noPrev)
//                            .For(y => y.ChangePercent, "% Change", 1, "+#'%';-#'%';0'%'", fordefault: "", hide: noPrev, aggregate: (r) => { var t = (double)r.Sum(z => z.Prev); return t > 0 ? ((r.Sum(z => z.Now) / t) - 1) * 100 : 0d; })
//                            .For(y => y.Additions, "Additions", 1, "+#,#;-#,#;0", fordefault: "", total: true, hide: noPrev)
//                            .For(y => y.Deletions, "Deletions", 1, "+#,#;-#,#;0", fordefault: "", total: true, hide: noPrev)
//                            .For(y => y.Same, hide: true)
//                            .For(y => y.SamePercentage, "% Same", 1, "#'%';-#'%';0'%'", fordefault: "", hide: noPrev, aggregate: (r) => { var t = (double)r.Sum(z => z.Now); return t > 0 ? r.Sum(z => z.Same) / t * 100 : 0d; })
//                            .For(y => y.Ids, hide: true)));
//                        sb.AppendLine("```");
//                    }
//                }

//                var msg = sb.ToString();
//                if (!string.IsNullOrWhiteSpace(msg)) await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
//            }
//        }
//    }
//}
