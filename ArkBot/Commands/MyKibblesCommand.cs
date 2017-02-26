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
        private DatabaseContextFactory<IEfDatabaseContext> _databaseContextFactory;

        public MyKibblesCommand(IArkContext context, IConstants constants, DatabaseContextFactory<IEfDatabaseContext> databaseContextFactory)
        {
            _context = context;
            _constants = constants;
            _databaseContextFactory = databaseContextFactory;
        }

        public void Register(CommandBuilder command) { }

        public async Task Run(CommandEventArgs e)
        {
            var player = await CommandHelper.GetCurrentPlayerOrSendErrorMessage(e, _databaseContextFactory, _context);
            if (player == null) return;

            var inv = (player.Inventory ?? new EntityNameWithCount[] { });
            if (player.TribeId.HasValue) inv = inv.Concat(_context.Tribes.FirstOrDefault(x => x.Id.HasValue && x.Id == player.TribeId.Value)?.Items ?? new EntityNameWithCount[] { }).ToArray();

            var _rEgg = new Regex(@"^(?<name>.+?)\s+Egg$", RegexOptions.Singleline);
            var _rKibble = new Regex(@"^Kibble\s+\((?<name>.+?)\s+Egg\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var kibbles = inv.Where(x => x.Name.StartsWith("Kibble", StringComparison.Ordinal))
                .GroupBy(x => x.Name)
                .Select(x => new EntityNameWithCount { Name = x.Key, Count = x.Sum(y => y.Count) })
                .OrderByDescending(x => x.Count)
                .ToArray();

            var eggs = inv.Where(x => x.Name.EndsWith("Egg", StringComparison.Ordinal))
                .GroupBy(x => x.Name)
                .Select(x => new EntityNameWithCount { Name = _rEgg.Match(x.Key, m => m.Success ? m.Groups["name"].Value : x.Key), Count = x.Sum(y => y.Count) })
                .OrderByDescending(x => x.Count)
                .ToArray();


            var results = kibbles.Select(x =>
            {
                var name = _rKibble.Match(x.Name, m => m.Success ? m.Groups["name"].Value : x.Name);
                var aliases = _context.SpeciesAliases.GetAliases(name);
                return new
                {
                    Name = name,
                    Count = x.Count,
                    EggCount = aliases == null || aliases.Length == 0 ? 0 : eggs.FirstOrDefault(y =>
                    {
                        return aliases.Contains(y.Name, StringComparer.OrdinalIgnoreCase);
                    })?.Count ?? 0
                };
            }).ToArray();

            if (results.Length <= 0)
            {
                await e.Channel.SendMessage($"<@{e.User.Id}>, {(player.TribeId.HasValue ? "your tribe have" : "you have")} no kibbles! :(");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"**{(player.TribeId.HasValue ? "Your tribe have" : "You have")} these kibbles**");

            sb.AppendLine("```");
            sb.AppendLine(FixedWidthTableHelper.ToString(results, x => x
                .For(y => y.Name, "Type")
                .For(y => y.EggCount, "Eggs", 1, "N0", total: true)
                .For(y => y.Count, "Kibbles", 1, "N0", total: true)));
            sb.AppendLine("```");

            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());
        }
    }
}
