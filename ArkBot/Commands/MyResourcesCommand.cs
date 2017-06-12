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

//namespace ArkBot.Commands
//{
//    public class MyResourcesCommand : ICommand
//    {
//        public string Name => "myresources";
//        public string[] Aliases => new[] { "mystuff", "myitems" };
//        public string Description => "Listing of your current resources";
//        public string SyntaxHelp => null;
//        public string[] UsageExamples => null;

//        public bool DebugOnly => false;
//        public bool HideFromCommandList => false;

//        private IConstants _constants;
//        private EfDatabaseContextFactory _databaseContextFactory;

//        public MyResourcesCommand(IConstants constants, EfDatabaseContextFactory databaseContextFactory)
//        {
//            _constants = constants;
//            _databaseContextFactory = databaseContextFactory;
//    }

//        public void Register(CommandBuilder command) { }

//        public void Init(DiscordClient client) { }

//        public async Task Run(CommandEventArgs e)
//        {
//            if (!_context.IsInitialized)
//            {
//                await e.Channel.SendMessage($"**The data is loading but is not ready yet...**");
//                return;
//            }

//            var player = await CommandHelper.GetCurrentPlayerOrSendErrorMessage(e, _databaseContextFactory, _context);
//            if (player == null) return;

//            var inv = (player.Inventory ?? new EntityNameWithCount[] { });
//            if (player.TribeId.HasValue) inv = inv.Concat(_context.Tribes.FirstOrDefault(x => x.Id.HasValue && x.Id == player.TribeId.Value)?.Items ?? new EntityNameWithCount[] { }).ToArray();


//            var result = Get(player?.Id, player?.TribeId);

//            if (result == null)
//            {
//                await e.Channel.SendMessage($"<@{e.User.Id}>, {(player.TribeId.HasValue ? "your tribe have" : "you have")} no resources! :(");
//                return;
//            }

//            var sb = new StringBuilder();
//            sb.AppendLine($"**{(player.TribeId.HasValue ? "Your tribe have" : "You have")} these major resources**");
//            sb.Append(result);

//            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
//        }

//        public string Get(int? playerId, int? tribeId)
//        {
//            if (playerId == null && tribeId == null) return null; //both cannot be null

//            var player = playerId.HasValue ? _context.Players?.FirstOrDefault(x => x.Id == playerId.Value) : null;
//            var tribe = tribeId.HasValue ? _context.Tribes?.FirstOrDefault(x => x.Id == tribeId.Value) : null;

//            var inv = new[] { player?.Inventory, tribe?.Items }.Where(x => x != null).SelectMany(x => x).ToArray();

//            var includedResources = new[]
//            {
//                "Hide", "Thatch", "Cementing Paste", "Fiber", "Narcotic", "Spoiled Meat", "Raw Meat",
//                "Wood", "Chitin", "Flint", "Silica Pearls", "Metal Ingot", "Obsidian", "Stone",
//                "Keratin", "Cooked Meat Jerky", "Oil", "Prime Meat Jerky", "Pelt", "Crystal",
//                "Narcoberry", "Mejoberry", "Stimberry",
//                "Amarberry", "Azulberry", "Tintoberry",
//                "Black Pearl", "Element"
//            };

//            var combined = new Dictionary<string, string[]> {
//                { "Chitin/Keratin",  new [] { "Chitin", "Keratin", "Chitin/Keratin" } },
//                { "Editable berries",  new [] { "Amarberry", "Azulberry", "Tintoberry", "Mejoberry" } }
//            };

//            var resources = inv.Where(x => includedResources.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
//                .GroupBy(x => x.Name)
//                .Select(x => new EntityNameWithCount { Name = x.Key, Count = x.Sum(y => y.Count) })
//                .ToList();

//            var n = combined.Select(c => new EntityNameWithCount { Name = c.Key, Count = resources.Where(x => c.Value.Contains(x.Name, StringComparer.OrdinalIgnoreCase)).Sum(x => x.Count) }).ToArray();
//            var remove = combined.SelectMany(y => y.Value).Except(new[] { "Mejoberry" }).ToArray();
//            resources.RemoveAll(x => remove.Contains(x.Name));
//            n = n.Where(x => x.Count > 0).ToArray();

//            resources = (n.Length > 0 ? resources.Concat(n) : resources).OrderBy(x => x.Name).ToList();

//            if (resources.Count == 0) return null;

//            var sb = new StringBuilder();
            
//            sb.AppendLine("```");
//            sb.AppendLine(FixedWidthTableHelper.ToString(resources, x => x
//                .For(y => y.Name, "Type")
//                .For(y => y.Count, null, 1, "N0")));
//            sb.AppendLine("```");

//            return sb.ToString();
//        }
//    }
//}
