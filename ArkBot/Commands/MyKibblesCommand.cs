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
using ArkBot.Data;
using ArkBot.Database;

namespace ArkBot.Commands
{
    public class MyKibblesCommand : ICommand
    {
        public string Name => "mykibbles";
        public string[] Aliases => new[] { "myeggs" };
        public string Description => "Listing of your kibbles and eggs";
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        private IConstants _constants;
        private IArkContext _context;
        private EfDatabaseContextFactory _databaseContextFactory;

        public MyKibblesCommand(IArkContext context, IConstants constants, EfDatabaseContextFactory databaseContextFactory)
        {
            _context = context;
            _constants = constants;
            _databaseContextFactory = databaseContextFactory;
        }

        public void Register(CommandBuilder command) { }

        public void Init(Discord.DiscordClient client) { }

        public async Task Run(CommandEventArgs e)
        {
            var myEggs = e.Message.Text.EndsWith("myeggs", StringComparison.OrdinalIgnoreCase);

            var player = await CommandHelper.GetCurrentPlayerOrSendErrorMessage(e, _databaseContextFactory, _context);
            if (player == null) return;

            //var inv = (player.Inventory ?? new EntityNameWithCount[] { });
            //if (player.TribeId.HasValue) inv = inv.Concat(_context.Tribes.FirstOrDefault(x => x.Id.HasValue && x.Id == player.TribeId.Value)?.Items ?? new EntityNameWithCount[] { }).ToArray();

            var result = Get(player?.Id, player?.TribeId, myEggs);

            var type = myEggs ? "eggs" : "kibbles";
            if (result == null)
            {
                await e.Channel.SendMessage($"<@{e.User.Id}>, {(player.TribeId.HasValue ? "your tribe have" : "you have")} no {type}! :(");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"**{(player.TribeId.HasValue ? "Your tribe have" : "You have")} these {type}**");
            sb.Append(result);

            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
        }

        internal string Get(int? playerId, int? tribeId, bool sortByEggs = false)
        {
            if (playerId == null && tribeId == null) return null; //both cannot be null

            var player = playerId.HasValue ? _context.Players?.FirstOrDefault(x => x.Id == playerId.Value) : null;
            var tribe = tribeId.HasValue ? _context.Tribes?.FirstOrDefault(x => x.Id == tribeId.Value) : null;

            var inv = new[] { player?.Inventory, tribe?.Items }.Where(x => x != null).SelectMany(x => x).ToArray();

            var _rEgg = new Regex(@"^(?<name>.+?)\s+Egg$", RegexOptions.Singleline);
            var _rKibble = new Regex(@"^Kibble\s+\((?<name>.+?)\s+Egg\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var kibbles = inv.Where(x => x.Name.StartsWith("Kibble", StringComparison.Ordinal))
                .GroupBy(x => x.Name)
                .Select(x => new EntityNameWithCount { Name = _rKibble.Match(x.Key, m => m.Success ? m.Groups["name"].Value : x.Key), Count = x.Sum(y => y.Count) })
                .ToArray();

            var eggs = inv.Where(x => x.Name.EndsWith("Egg", StringComparison.Ordinal) && !x.Name.StartsWith("Fertilized"))
                .GroupBy(x => x.Name)
                .Select(x => new EntityNameWithCount { Name = _rEgg.Match(x.Key, m => m.Success ? m.Groups["name"].Value : x.Key), Count = x.Sum(y => y.Count) })
                .ToList();

            var results = kibbles.Select(x =>
            {
                var aliases = _context.SpeciesAliases.GetAliases(x.Name);
                var egg = aliases == null || aliases.Length == 0 ? null : eggs.FirstOrDefault(y =>
                {
                    return aliases.Contains(y.Name, StringComparer.OrdinalIgnoreCase);
                });
                if (egg != null) eggs.Remove(egg);
                return new
                {
                    Name = x.Name,
                    Count = x.Count,
                    EggCount = egg?.Count ?? 0
                };
            }).Concat(eggs.Select(x => new
            {
                Name = x.Name,
                Count = 0,
                EggCount = x.Count
            })).OrderByDescending(x => sortByEggs ? x.EggCount : x.Count).ThenByDescending(x => sortByEggs ? x.Count : x.EggCount).ToArray();

            if (results.Length == 0) return null;

            var sb = new StringBuilder();
            sb.AppendLine("```");
            sb.AppendLine(FixedWidthTableHelper.ToString(results, x => x
                .For(y => y.Name, "Type")
                .For(y => y.EggCount, "Eggs", 1, "N0", total: true)
                .For(y => y.Count, "Kibbles", 1, "N0", total: true)));
            sb.AppendLine("```");

            return sb.ToString();
        }
    }
}
