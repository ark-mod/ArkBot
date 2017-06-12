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
//using ArkBot.Data;
//using ArkBot.Database;
//using Discord;
//using ArkBot.Ark;

//namespace ArkBot.Commands
//{
//    public class MyKibblesCommand : ICommand
//    {
//        public string Name => "mykibbles";
//        public string[] Aliases => new[] { "myeggs" };
//        public string Description => "Listing of your kibbles and eggs";
//        public string SyntaxHelp => "<***server key***>";
//        public string[] UsageExamples => new[]
//        {
//            "**<server key>**: Listing of your kibbles and eggs from specified server instance."
//        };

//        public bool DebugOnly => false;
//        public bool HideFromCommandList => false;

//        private IConstants _constants;
//        private EfDatabaseContextFactory _databaseContextFactory;
//        private ArkContextManager _contextManager;

//        public MyKibblesCommand(
//            IConstants constants, 
//            EfDatabaseContextFactory databaseContextFactory,
//            ArkContextManager contextManager)
//        {
//            _constants = constants;
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
//            var args = CommandHelper.ParseArgs(e, new { ServerKey = "" }, x =>
//                x.For(y => y.ServerKey, noPrefix: true, isRequired: true));

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

//            var myEggs = e.Message.Text.EndsWith("myeggs", StringComparison.OrdinalIgnoreCase);

//            var player = await CommandHelper.GetCurrentPlayerOrSendErrorMessage(e, _databaseContextFactory, serverContext);
//            if (player == null) return;

//            //var inv = (player.Inventory ?? new EntityNameWithCount[] { });
//            //if (player.TribeId.HasValue) inv = inv.Concat(_context.Tribes.FirstOrDefault(x => x.Id.HasValue && x.Id == player.TribeId.Value)?.Items ?? new EntityNameWithCount[] { }).ToArray();

//            var result = Get(serverContext, player?.Id, player?.TribeId, myEggs);

//            var type = myEggs ? "eggs" : "kibbles";
//            if (result == null)
//            {
//                await e.Channel.SendMessage($"<@{e.User.Id}>, {(player.TribeId.HasValue ? "your tribe have" : "you have")} no {type}! :(");
//                return;
//            }

//            var sb = new StringBuilder();
//            sb.AppendLine($"**{(player.TribeId.HasValue ? "Your tribe have" : "You have")} these {type}**");
//            sb.Append(result);

//            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
//        }

//        internal string Get(ArkServerContext serverContext, int? playerId, int? tribeId, bool sortByEggs = false)
//        {
//            if (playerId == null && tribeId == null) return null; //both cannot be null

//            var player = playerId.HasValue ? serverContext.Players?.FirstOrDefault(x => x.Id == playerId.Value) : null;
//            var tribe = tribeId.HasValue ? serverContext.Tribes?.FirstOrDefault(x => x.Id == tribeId.Value) : null;

//            var inv = new[] { player?.Inventory, tribe?.Items }.Where(x => x != null).SelectMany(x => x).ToArray();

//            var _rEgg = new Regex(@"^(?<name>.+?)\s+Egg$", RegexOptions.Singleline);
//            var _rKibble = new Regex(@"^Kibble\s+\((?<name>.+?)\s+Egg\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

//            var kibbles = inv.Where(x => x.ClassName.StartsWith("PrimalItemConsumable_Kibble", StringComparison.Ordinal))
//                .GroupBy(x => x.ClassName)
//                .Select(x => new EntityNameWithCount { Name = _rKibble.Match(x.Key, m => m.Success ? m.Groups["name"].Value : x.Key), Count = x.Sum(y => y.Quantity) })
//                .ToArray();

//            var eggs = inv.Where(x => x.ClassName.EndsWith("PrimalItemConsumable_Egg", StringComparison.Ordinal) && !x.ClassName.EndsWith("_Fertilized_C"))
//                .GroupBy(x => x.ClassName)
//                .Select(x => new EntityNameWithCount { Name = _rEgg.Match(x.Key, m => m.Success ? m.Groups["name"].Value : x.Key), Count = x.Sum(y => y.Quantity) })
//                .ToList();

//            var results = kibbles.Select(x =>
//            {
//                var aliases = ArkSpeciesAliases.Instance.GetAliases(x.Name);
//                var egg = aliases == null || aliases.Length == 0 ? null : eggs.FirstOrDefault(y =>
//                {
//                    return aliases.Contains(y.Name, StringComparer.OrdinalIgnoreCase);
//                });
//                if (egg != null) eggs.Remove(egg);
//                return new
//                {
//                    Name = x.Name,
//                    Count = x.Count,
//                    EggCount = egg?.Count ?? 0
//                };
//            }).Concat(eggs.Select(x => new
//            {
//                Name = x.Name,
//                Count = 0L,
//                EggCount = x.Count
//            })).OrderByDescending(x => sortByEggs ? x.EggCount : x.Count).ThenByDescending(x => sortByEggs ? x.Count : x.EggCount).ToArray();

//            if (results.Length == 0) return null;

//            var sb = new StringBuilder();
//            sb.AppendLine("```");
//            sb.AppendLine(FixedWidthTableHelper.ToString(results, x => x
//                .For(y => y.Name, "Type")
//                .For(y => y.EggCount, "Eggs", 1, "N0", total: true)
//                .For(y => y.Count, "Kibbles", 1, "N0", total: true)));
//            sb.AppendLine("```");

//            return sb.ToString();
//        }
//    }
//}
