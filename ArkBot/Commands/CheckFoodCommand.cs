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
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
using ArkBot.Database;

namespace ArkBot.Commands
{
    public class CheckFoodCommand : ICommand
    {
        public string Name => "checkfood";
        public string[] Aliases => new[] { "food" };
        public string Description => "Summary of the food status of your personal- and tribe owned dinos";
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => false;
        public bool HideFromCommandList => false;

        public void Register(CommandBuilder command) { }

        private IConfig _config;
        private IArkContext _context;
        private IConstants _constants;
        private DatabaseContextFactory<IEfDatabaseContext> _databaseContextFactory;

        public CheckFoodCommand(IConfig config, IArkContext context, IConstants constants, DatabaseContextFactory<IEfDatabaseContext> databaseContextFactory)
        {
            _config = config;
            _context = context;
            _constants = constants;
            _databaseContextFactory = databaseContextFactory;
        }

        public async Task Run(CommandEventArgs e)
        {
            var player = await CommandHelper.GetCurrentPlayerOrSendErrorMessage(e, _databaseContextFactory, _context);
            if (player == null) return;

            var mydinos = _context.CreaturesNoRaft
                .Where(x => (x.PlayerId.HasValue && x.PlayerId.Value == player.Id) || (x.Team.HasValue && x.Team.Value == player.TribeId))
                .Select(x =>
                {
                    //!_context.ArkSpeciesStatsData.SpeciesStats.Any(y => y.Name.Equals(x.SpeciesName, StringComparison.OrdinalIgnoreCase)) ? _context.ArkSpeciesStatsData.SpeciesStats.Select(y => new { name = y.Name, similarity = StatisticsHelper.CompareToCharacterSequence(x.Name, x.SpeciesName.ToCharArray()) }).OrderByDescending(y => y.similarity).FirstOrDefault()?.name : null;
                    return new
                    {
                        creature = x,
                        maxFood = _context.CalculateMaxFood(x.SpeciesClass ?? x.SpeciesName, x.WildLevels?.Food, x.TamedLevels?.Food, x.ImprintingQuality)
                    };
                })
                .ToArray();
            var foodStatus = mydinos?.Where(x => x.creature.CurrentFood.HasValue && x.maxFood.HasValue).Select(x => (double)x.creature.CurrentFood.Value / x.maxFood.Value)
                .Where(x => x <= 1d).OrderBy(x => x).ToArray();
            var starving = mydinos?.Where(x => x.creature.CurrentFood.HasValue && x.maxFood.HasValue).Select(x => new { creature = x.creature, p = (double)x.creature.CurrentFood.Value / x.maxFood.Value })
                .Where(x => x.p <= (1 / 2d)).OrderBy(x => x.p).ToArray(); //any dino below 1/2 food is considered to be starving
            //todo: babys are not idenftified in this code and as such are always considered to be starving
            if (foodStatus.Length <= 0)
            {
                await e.Channel.SendMessage($"<@{e.User.Id}>, we could not get the food status of your dinos! :(");
                return;
            }

            var min = foodStatus.Min();
            var avg = foodStatus.Average();
            var max = foodStatus.Max();

            var stateFun = min <= 0.25 ? "starving... :(" : min <= 0.5 ? "hungry... :|" : min <= 0.75 ? "feeling satisfied :)" : "feeling happy! :D";

            var sb = new StringBuilder();
            sb.AppendLine($"**Your dinos are {stateFun}**");
            sb.AppendLine($"{min:P0} ≤ {avg:P0} ≤ {max:P0}");
            if (starving.Length > 0)
            {
                var tmp = starving.Select(x => $"{x.creature.Name ?? x.creature.SpeciesName}, lvl {x.creature.FullLevel ?? x.creature.BaseLevel}" + $" ({x.p:P0})").ToArray().Join((n, l) => n == l ? " and " : ", ");
                sb.AppendLine(tmp);
            }

            await CommandHelper.SendPartitioned(e.Channel, sb.ToString());

            var filepath = Path.Combine(_config.TempFileOutputDirPath, "chart.jpg");
            if (CommandHelper.AreaPlotSaveAs(foodStatus.Select((x, i) => new DataPoint(i, x)).ToArray(), filepath))
            {
                await e.Channel.SendFile(filepath);
            }
            if (File.Exists(filepath)) File.Delete(filepath);

            
        }
    }
}