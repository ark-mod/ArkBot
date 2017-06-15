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
//using Discord;
//using ArkBot.Ark;

//namespace ArkBot.Commands
//{
//    public class StatsCommand : ICommand
//    {
//        public string Name => "stats";
//        public string[] Aliases => new[] { "statistics", "top" };
//        public string Description => "Get tribe/player statistics";
//        public string SyntaxHelp => "<***server key***> [***tribe <name>***] [***player <name>***] [***skip <number>***]";
//        public string[] UsageExamples => new[]
//        {
//            "**<server key>**: Statistics for the top 10 tribes by tamed dino count",
//            "**<server key>** **tribe epic**: Statistics for the ***tribe 'epic'***",
//            "**<server key>** **player nils**: Statistics for the ***player 'nils'***"
//        };

//        public bool DebugOnly => false;
//        public bool HideFromCommandList => false;

//        private ArkContextManager _contextManager;

//        public StatsCommand(ArkContextManager contextManager)
//        {
//            _contextManager = contextManager;
//        }

//        public void Register(CommandBuilder command)
//        {
//            command.Parameter("optional", ParameterType.Multiple);
//        }

//        public void Init(DiscordClient client) { }

//        public async Task Run(CommandEventArgs e)
//        {
//            var take = 10;
//            var args = CommandHelper.ParseArgs(e, new { ServerKey = "", Tribe = "", Player = "", Skip = 0 }, x =>
//                x.For(y => y.ServerKey, noPrefix: true, isRequired: true)
//                .For(y => y.Tribe, untilNextToken: true)
//                .For(y => y.Player, untilNextToken: true)
//                .For(y => y.Skip, defaultValue: 0));
//            if (args == null || args.Skip < 0 || string.IsNullOrEmpty(args.ServerKey))
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

//            var sb = new StringBuilder();
//            var filtered = serverContext.InclCloudNoRafts;

//            var tribe = args.Tribe;
//            var player = args.Player;

//            if (tribe != null)
//            {
//                if (serverContext.Tribes.Count(x => x.Name != null && x.Name.Equals(tribe, StringComparison.OrdinalIgnoreCase)) > 1)
//                {
//                    await e.Channel.SendMessage($"**There is more than one tribe with the specified name! :(**");
//                    return;
//                }

//                filtered = filtered.Where(x => x.TribeName != null && x.TribeName.Equals(tribe, StringComparison.OrdinalIgnoreCase));
//            }
//            else if (player != null)
//            {
//                if (serverContext.Players.Count(x => x.Name != null && x.Name.Equals(player, StringComparison.OrdinalIgnoreCase)) > 1)
//                {
//                    await e.Channel.SendMessage($"**There is more than one player with the specified name! :(**");
//                    return;
//                }

//                var tribes = serverContext.Tribes.Where(x => x.MemberNames.Contains(player, StringComparer.OrdinalIgnoreCase)).ToArray();
//                if (tribes.Length == 1)
//                {
//                    //this is what the user expect when they issue the command
//                    //if they are in a tribe they want the tribe stats (usually one member tribes)
//                    //note that this makes it impossible to return stats for a single member in a tribe with "player owned" set.
//                    //also note that ark-tools give us a list of player names as members of a tribe, but since we already returned if more than one player with the same name was found it is not an issue.

//                    player = null;
//                    tribe = tribes.First().Name;
//                    filtered = filtered.Where(x => x.TribeName != null && x.TribeName.Equals(tribe, StringComparison.OrdinalIgnoreCase));
//                }
//                else
//                {
//                    filtered = filtered.Where(x => x.OwningPlayerName != null && x.OwningPlayerName.Equals(player, StringComparison.OrdinalIgnoreCase));
//                }
//            }

//            var groups = filtered.GroupBy(x => player != null ? x.OwningPlayerId : x.TargetingTeam)
//                .OrderByDescending(x => x.Count(z => !(z is ArkSavegameToolkitNet.Domain.ArkCloudInventoryDino))).Skip(tribe == null && player == null ? args.Skip : 0).Take(take).Select(x =>
//                {
//                    var species = x.GroupBy(y => y.ClassName)
//                            .Select(y => new Tuple<string, int, int>(Data.ArkSpeciesAliases.Instance.GetAliases(y.Key)?.FirstOrDefault() ?? y.Key, y.Count(z => !(z is ArkSavegameToolkitNet.Domain.ArkCloudInventoryDino)), y.Count(z => z is ArkSavegameToolkitNet.Domain.ArkCloudInventoryDino))).OrderByDescending(y => y.Item2).ToArray();
//                    var structures = player != null ?
//                        null /*_context.Players?.Where(y => y.Id == x.Key).SelectMany(y => y.Structures).Sum(y => y.Count)*/
//                        : serverContext.Structures?.Where(y => y.TargetingTeam == x.Key).GroupBy(y => y.ClassName).Select(y => new { className = y.Key, count = y.Count() }).ToArray();
//                    var structuresCount = structures?.Sum(y => y.count);
//                    return new
//                    {
//                        key = x.Key,
//                        name = player != null ? x.FirstOrDefault()?.OwningPlayerName : x.FirstOrDefault()?.TribeName,
//                        num = x.Count(z => !(z is ArkSavegameToolkitNet.Domain.ArkCloudInventoryDino)),
//                        numCluster = x.Count(z => z is ArkSavegameToolkitNet.Domain.ArkCloudInventoryDino),
//                        species = tribe != null || player != null ? species : species.Length > 1 ? StatisticsHelper.FilterUsingStandardDeviation(species, z => z.Item2, (dist, sd) => dist >= sd, true) : species,
//                        distinctSpeciesCount = species.Length,
//                        structuresCount = structuresCount,
//                        structures = tribe != null || player != null ? StatisticsHelper.FilterUsingStandardDeviation(structures, z => z.count, (dist, sd) => dist >= 0, true) : null
//                    };
//                }).ToArray();

//            if (tribe == null && player == null)
//            {
//                sb.AppendLine("**Statistics per Tribe (showing top " + (args.Skip == 0 ? groups.Length.ToString() : $"{args.Skip + 1}-{args.Skip + groups.Length}") + ")**");
//            }

//            var rank = tribe == null && player == null ? args.Skip : 0;
//            foreach (var t in groups)
//            {
//                sb.AppendLine("**" + (tribe == null && player == null ? $"{rank + 1}. " : "") + $"{(!string.IsNullOrWhiteSpace(t.name) ? t.name : t.key.ToString())} have a total of {t.num:N0} tamed dinos{(t.numCluster > 0 ? $" (+{t.numCluster:N0} in cluster)" : "")}, {t.distinctSpeciesCount} distinct species" + (t.structuresCount.HasValue ? $" and {t.structuresCount:N0} structures" : "") + "**");
//                if (t.species?.Length > 0) sb.AppendLine((tribe == null && player == null ? "which includes *" : "") + t.species.Select(x => $"{x.Item2:N0}{(x.Item3 > 0 ? $" (+{x.Item3:N0})" : "")} {x.Item1}").ToArray().Join((n, l) => n == l ? " and " : ", ") + (tribe == null && player == null ? "*" : ""));
//                if (t.structures != null && t.structures.Length > 0)
//                {
//                    sb.AppendLine().AppendLine("**Most Common Structures**");
//                    sb.AppendLine(t.structures.Select(x => $"{x.count:N0} {x.className}").ToArray().Join((n, l) => n == l ? " and " : ", "));
//                }
//                if (tribe != null)
//                {
//                    //top resources
//                    var includedResources = new[]
//                    {
//                        "Hide", "Thatch", "Cementing Paste", "Fiber", "Narcotic", "Spoiled Meat", "Raw Meat",
//                        "Wood", "Chitin", "Flint", "Silica Pearls", "Metal Ingot", "Obsidian", "Stone",
//                        "Keratin", "Cooked Meat Jerky", "Oil", "Prime Meat Jerky", "Pelt", "Crystal",
//                        "Narcoberry", "Mejoberry", "Stimberry",
//                        "Amarberry", "Azulberry", "Tintoberry",
//                        "Black Pearl", "Element"
//                    };

//                    //todo: items are tough to associate with a particular player or tribe with the new extracted data
//                    //var resources = serverContext.Tribes.Where(x => x.Id != t.key).GroupBy(x => x.Id).Select(x => new { Id = x.Key, Name = x.FirstOrDefault()?.Name, Items = x.SelectMany(y => y.Items).GroupBy(y => y.Name).Select(y => new { Name = y.Key, Count = y.Sum(z => z.Count) }).ToArray() }).ToArray();
//                    var prevRank = 0;
//                    //var items = serverContext.Items?.Where(x => x.Id == t.key)
//                    //    .SelectMany(x => x.Items)
//                    //    .Where(x => includedResources.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
//                    //    .Select(x => {
//                    //        var opponents = resources.Select(y => new
//                    //        {
//                    //            Id = y.Id,
//                    //            Name = y.Name,
//                    //            Count = y.Items.Where(z => z.Name.Equals(x.Name, StringComparison.Ordinal)).Sum(z => z.Count)
//                    //        }).OrderByDescending(y => y.Count).ToArray();
//                    //        return new
//                    //        {
//                    //            Name = x.Name,
//                    //            Rank = opponents.Count(y => y.Count > x.Count)
//                    //        };
//                    //    }).OrderBy(x => x.Rank).TakeWhile((x, i) => { var y = i < 100 && (i < 25 || (prevRank == x.Rank)); prevRank = x.Rank; return y; }).GroupBy(x => x.Rank).ToArray();

//                    //if (items != null && items.Length > 0)
//                    //{
//                    //    sb.AppendLine().AppendLine("**Top Resources / server ranking**");
//                    //    foreach(var g in items) sb.AppendLine($"**#{(g.Key + 1):N0}** " + g.Select(x => x.Name).ToArray().Join((n, l) => n == l ? " and " : ", "));
//                    //}

//                    //top dinos
//                    var dinos = serverContext.InclCloudNoRafts.Where(x => x.TargetingTeam == t.key).GroupBy(x => x.ClassName).Select(x => {
//                        var baseLevel = x.Max(y => y.BaseLevel);
//                        var level = x.Max(y => y.BaseLevel + y.ExtraCharacterLevel);
//                        var opponents = serverContext.InclCloudNoRafts.Where(y => y.ClassName.Equals(x.Key, StringComparison.OrdinalIgnoreCase))
//                                .GroupBy(y => y.TargetingTeam);
//                        var name = x.FirstOrDefault()?.ClassName;
//                        name = Data.ArkSpeciesAliases.Instance.GetAliases(name)?.FirstOrDefault() ?? name;
//                        return new
//                        {
//                            Class = x.Key,
//                            Name = name,
//                            BaseLevel = baseLevel,
//                            Level = level,
//                            BaseLevelRank = opponents.Select(y => y.Max(z => z.BaseLevel)).Count(y => y > baseLevel),
//                            LevelRank = opponents.Select(y => y.Max(z => z.BaseLevel + z.ExtraCharacterLevel)).Count(y => y > level)
//                        };
//                    }).ToArray();

//                    if (dinos != null && dinos.Length > 0)
//                    {
//                        prevRank = 0;
//                        var baseLevel = dinos.OrderBy(x => x.BaseLevelRank).TakeWhile((x, i) => { var y = i < 100 && (i < 25 || (prevRank == x.BaseLevelRank)); prevRank = x.BaseLevelRank; return y; }).GroupBy(x => x.BaseLevelRank).ToArray();

//                        prevRank = 0;
//                        var level = dinos.OrderBy(x => x.LevelRank).TakeWhile((x, i) => { var y = i < 100 && (i < 25 || (prevRank == x.LevelRank)); prevRank = x.LevelRank; return y; }).GroupBy(x => x.LevelRank).ToArray();

//                        sb.AppendLine().AppendLine("**Top tamed dinos by Base Level / server ranking**");
//                        foreach (var g in baseLevel) sb.AppendLine($"**#{(g.Key + 1):N0}** " + g.Select(x => x.Name).ToArray().Join((n, l) => n == l ? " and " : ", "));

//                        sb.AppendLine().AppendLine("**Top tamed dinos by Current Level / server ranking**");
//                        foreach (var g in level) sb.AppendLine($"**#{(g.Key + 1):N0}** " + g.Select(x => x.Name).ToArray().Join((n, l) => n == l ? " and " : ", "));
//                    }
//                }
//                if (tribe == null && player == null) sb.AppendLine();
//                rank++;
//            }

//            if (filtered.Count() <= 0)
//            {
//                await e.Channel.SendMessage($"**I could not find any statistics with the given parameters... :(**");
//            }
//            else
//            {
//                await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
//            }
//        }
//    }
//}
