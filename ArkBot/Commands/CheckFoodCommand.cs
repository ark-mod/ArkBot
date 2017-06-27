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
//using System.IO;
//using System.Windows.Forms.DataVisualization.Charting;
//using ArkBot.Database;
//using Discord;
//using ArkBot.Ark;
//using ArkSavegameToolkitNet.Domain;
//using ArkBot.Data;

//namespace ArkBot.Commands
//{
//    public class CheckFoodCommand : ICommand
//    {
//        public string Name => "checkfood";
//        public string[] Aliases => new[] { "food" };
//        public string Description => "Summary of the food status of your personal- and tribe owned dinos";
//        public string SyntaxHelp => "[<***server key***>]";
//        public string[] UsageExamples => new[]
//        {
//            ": Food status from main server",
//            "**<server key>**: Food status from specified server instance."
//        };

//        public bool DebugOnly => false;
//        public bool HideFromCommandList => false;

//        public void Register(CommandBuilder command)
//        {
//            command.Parameter("optional", ParameterType.Multiple);
//        }

//        public void Init(DiscordClient client) { }

//        private IConfig _config;
//        private IConstants _constants;
//        private EfDatabaseContextFactory _databaseContextFactory;
//        private ArkContextManager _contextManager;

//        public CheckFoodCommand(
//            IConfig config, 
//            IConstants constants, 
//            EfDatabaseContextFactory databaseContextFactory,
//            ArkContextManager contextManager)
//        {
//            _config = config;
//            _constants = constants;
//            _databaseContextFactory = databaseContextFactory;
//            _contextManager = contextManager;
//        }

//        public async Task Run(CommandEventArgs e)
//        {
//            var args = CommandHelper.ParseArgs(e, new { ServerKey = "" }, x =>
//                x.For(y => y.ServerKey, noPrefix: true, isRequired: false));

//            var serverContext = _contextManager.GetServer(args.ServerKey ?? _config.ServerKey);
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


//            var playercreatures = serverContext.NoRafts.Where(x => x.TargetingTeam == player.Id || (x.OwningPlayerId.HasValue && x.OwningPlayerId == player.Id)).ToArray();
//            var tribecreatures = player.TribeId.HasValue ? serverContext.NoRafts.Where(x => x.TargetingTeam == player.TribeId.Value && !playercreatures.Any(y => y.Id == x.Id)).ToArray() : new ArkTamedCreature[] { };

//            var mydinos = playercreatures.Select(x => new { c = x, o = "player" }).Concat(tribecreatures.Select(x => new { c = x, o = "tribe" })).Select(item =>
//            {
//                var currentFood = item.c.CurrentStatusValues?.Length > 4 ? item.c.CurrentStatusValues[4] : null;
//                var maxFood = item.c.BaseStats?.Length > 4 && item.c.TamedStats?.Length > 4 ?
//                    ArkDataHelper.CalculateMaxStat(
//                        ArkSpeciesStatsData.Stat.Food,
//                        item.c.ClassName,
//                        item.c.BaseStats[4],
//                        item.c.TamedStats[4],
//                        (decimal)(item.c.DinoImprintingQuality ?? 0f),
//                        (decimal)(item.c.TamedIneffectivenessModifier ?? 0f)) : null;

//                //baby food formula: max * 0.1 + (max - (max * 0.1)) * age
//                if (maxFood.HasValue && item.c.BabyAge.HasValue) maxFood = maxFood.Value * 0.1 + (maxFood.Value - (maxFood.Value * 0.1)) * item.c.BabyAge.Value;

//                var fs = currentFood.HasValue && maxFood.HasValue ? currentFood.Value / (float)maxFood.Value : (float?)null;
//                if (fs.HasValue && fs > 1f) fs = 1f;

//                var aliases = ArkSpeciesAliases.Instance.GetAliases(item.c.ClassName);
//                return new { c = item.c, o = item.o, food = fs };
//            }).ToArray();

//            //var mydinos = serverContext.NoRafts
//            //    .Where(x => ((x.OwningPlayerId.HasValue && x.OwningPlayerId.Value == player.Id) || (x.Team.Value == player.TribeId)) && !x.IsBaby)
//            //    .Select(x =>
//            //    {
//            //        //!_context.ArkSpeciesStatsData.SpeciesStats.Any(y => y.Name.Equals(x.SpeciesName, StringComparison.OrdinalIgnoreCase)) ? _context.ArkSpeciesStatsData.SpeciesStats.Select(y => new { name = y.Name, similarity = StatisticsHelper.CompareToCharacterSequence(x.Name, x.SpeciesName.ToCharArray()) }).OrderByDescending(y => y.similarity).FirstOrDefault()?.name : null;
//            //        return new
//            //        {
//            //            creature = x,
//            //            maxFood = ArkDataHelper.CalculateMaxStat(Data.ArkSpeciesStatsData.Stat.Food, x.SpeciesClass ?? x.SpeciesName, x.WildLevels?.Food, x.TamedLevels?.Food, x.ImprintingQuality, x.TamedIneffectivenessModifier)
//            //        };
//            //    })
//            //    .ToArray();
//            //var foodStatus = mydinos?.Where(x => x.creature.CurrentFood.HasValue && x.maxFood.HasValue).Select(x => (double)x.creature.CurrentFood.Value / x.maxFood.Value)
//            //    .Where(x => x <= 1d).OrderBy(x => x).ToArray();
//            //var starving = mydinos?.Where(x => x.creature.CurrentFood.HasValue && x.maxFood.HasValue).Select(x => new { creature = x.creature, p = (double)x.creature.CurrentFood.Value / x.maxFood.Value })
//            //    .Where(x => x.p <= (1 / 2d)).OrderBy(x => x.p).ToArray(); //any dino below 1/2 food is considered to be starving
//            var foodStatus = mydinos.Where(x => x.food.HasValue).Select(x => x.food.Value).OrderBy(x => x).ToArray();
//            var starving = mydinos.Where(x => !x.food.HasValue || x.food <= (1 / 2d)).OrderBy(x => x.food).ToArray(); //any dino below 1/2 food is considered to be starving
//            if (foodStatus.Length <= 0)
//            {
//                await e.Channel.SendMessage($"<@{e.User.Id}>, we could not get the food status of your dinos! :(");
//                return;
//            }

//            var min = foodStatus.Min();
//            var avg = foodStatus.Average();
//            var max = foodStatus.Max();

//            var stateFun = min <= 0.25 ? "starving... :(" : min <= 0.5 ? "hungry... :|" : min <= 0.75 ? "feeling satisfied :)" : "feeling happy! :D";

//            var sb = new StringBuilder();
//            sb.AppendLine($"**Your dinos are {stateFun}**");
//            sb.AppendLine($"{min:P0} ≤ {avg:P0} ≤ {max:P0}");
//            if (starving.Length > 0)
//            {
//                var tmp = starving.Select(x =>
//                {
//                    var aliases = ArkSpeciesAliases.Instance.GetAliases(x.c.ClassName);
//                    return $"{x.c.Name ?? aliases.FirstOrDefault() ?? x.c.ClassName}, lvl {x.c.Level}" + $" ({(x.food.HasValue ? x.food.Value.ToString("P0") : "Unknown")})";
//                }).ToArray().Join((n, l) => n == l ? " and " : ", ");
//                sb.AppendLine(tmp);
//            }

//            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());

//            var filepath = Path.Combine(_config.TempFileOutputDirPath, "chart.jpg");
//            if (CommandHelper.AreaPlotSaveAs(foodStatus.Select((x, i) => new DataPoint(i, x)).ToArray(), filepath))
//            {
//                await e.Channel.SendFile(filepath);
//            }
//            if (File.Exists(filepath)) File.Delete(filepath);

            
//        }
//    }
//}