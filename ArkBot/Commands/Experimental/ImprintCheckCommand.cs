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
using System.Globalization;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace ArkBot.Commands.Experimental
{
    public class ImprintCheckCommand : ICommand
    {
        public string Name => "imprintcheck";
        public string[] Aliases => null;
        public string Description => "Check if a 100% imprint is likely to succeed or fail with the given stats";
        public string SyntaxHelp => null;
        public string[] UsageExamples => null;

        public bool DebugOnly => true;
        public bool HideFromCommandList => false;

        private IConfig _config;
        private IArkContext _context;

        public ImprintCheckCommand(IConfig config, IArkContext context)
        {
            _config = config;
            _context = context;
        }

        public void Register(CommandBuilder command)
        {
            command.Parameter("species", ParameterType.Required)
                .Parameter("imprintPercentage", ParameterType.Required)
                .Parameter("currentWeight", ParameterType.Required)
                .Parameter("maxWeight", ParameterType.Required)
                .Parameter("timeUntilNextImprint", ParameterType.Multiple);
        }

        public async Task Run(CommandEventArgs e)
        {
            var species = e.GetArg("species");
            var speciesNames = _context.SpeciesAliases?.GetAliases(species);
            if (speciesNames?.Length <= 0) return;

            species = speciesNames.FirstOrDefault(x => x.Equals(species, StringComparison.OrdinalIgnoreCase)) ?? species;

            double imprintPercentage = 0d;
            if (double.TryParse(e.GetArg("imprintPercentage")?.Replace(",", ".").Replace("%", "").Trim() ?? "", NumberStyles.Float & ~NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out imprintPercentage))
            {
                if (e.GetArg("imprintPercentage")?.IndexOf('%') == -1) imprintPercentage *= 100d;
            }
            else return;

            double currentWeight = 0d;
            if (!double.TryParse(e.GetArg("currentWeight")?.Replace(",", ".") ?? "", NumberStyles.Float & (NumberStyles.Float & ~NumberStyles.AllowExponent), CultureInfo.InvariantCulture, out currentWeight)) return;

            double maxWeight = 0d;
            if (!double.TryParse(e.GetArg("maxWeight")?.Replace(",", ".") ?? "", NumberStyles.Float & ~NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out maxWeight)) return;

            if (e.Command["timeUntilNextImprint"] == null) return;
            var timeUntilNextImprint = TimeSpanHelper.ParseFromString(string.Join(" ", e.Args.Skip(e.Command["timeUntilNextImprint"].Id).ToArray()));
            if (timeUntilNextImprint?.TotalSeconds < 0) return;

            var breedingStats = _context.ArkSpeciesStatsData?.SpeciesStats.FirstOrDefault(x => speciesNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))?.Breeding.GetAdjusted(_config.ArkMultipliers);
            if (breedingStats == null) return;

            var grownFraction = currentWeight / maxWeight;

            var imprintBonus = Math.Round(
                Math.Round(imprintPercentage * breedingStats.MaturationTime / (14400 * _config.ArkMultipliers.CuddleIntervalMultiplier))
                * 14400 * _config.ArkMultipliers.CuddleIntervalMultiplier / (breedingStats.MaturationTime)
            , 4);

            var now = DateTime.Now;
            var nextImprintWhen = now + timeUntilNextImprint.Value;
            var maturationFinishedWhen = now.AddSeconds(breedingStats.MaturationTime - (breedingStats.MaturationTime * grownFraction));

            //todo: is this correct?
            var cuddlesTotal = (int)Math.Ceiling((breedingStats.MaturationTime / (14400 * _config.ArkMultipliers.CuddleIntervalMultiplier)));
            var cuddlesGiven = (int)Math.Round((imprintBonus / 100) * cuddlesTotal);
            var cuddlesRemaining = cuddlesTotal - cuddlesGiven;

            if (cuddlesGiven >= cuddlesTotal) return;

            var totalSecondsSinceEpoch = new Func<DateTime, double>((dt) => dt.ToUniversalTime().Subtract(
                    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                ).TotalSeconds);

            var data = Enumerable.Range(cuddlesGiven + 1, cuddlesRemaining).Select(x => new
            {
                cuddle = x,
                dates = new[]
                {
                    nextImprintWhen.AddSeconds(x == cuddlesGiven + 1 ?
                       3600
                        : (x - (cuddlesGiven + 1)) * (10800 * _config.ArkMultipliers.CuddleIntervalMultiplier)),
                    nextImprintWhen.AddSeconds((x - (cuddlesGiven + 1)) * (14400 * _config.ArkMultipliers.CuddleIntervalMultiplier))
                }
            }).ToArray();

            //Debug.WriteLine($"Next imprint at {nextImprintWhen:yyyy-MM-dd HH:mm}");
            //Debug.WriteLine($"Fully grown at {maturationFinishedWhen:yyyy-MM-dd HH:mm}");
            //Debug.WriteLine($"{cuddlesGiven} of {cuddlesTotal} given ({cuddlesRemaining} remain)");
            //foreach (var d in data)
            //{
            //    Debug.WriteLine($"Cuddle #{d.cuddle}: {d.dates[0]:yyyy-MM-dd HH:mm} - {d.dates[1]:yyyy-MM-dd HH:mm}");
            //}

            var noChart = false;
            var finalCuddle = data.LastOrDefault()?.dates;
            string fullImprintProbabilityText = null;
            if (finalCuddle != null && finalCuddle.Length >= 2)
            {
                if (finalCuddle.First() > maturationFinishedWhen)
                {
                    fullImprintProbabilityText = " but sadly it will not reach 100% imprint... :(";
                    noChart = true;
                }
                else
                {
                    var finalCuddleMean = finalCuddle.First().AddSeconds((finalCuddle.Last() - finalCuddle.First()).TotalSeconds / 2d);
                    var diff = maturationFinishedWhen - finalCuddleMean;
                    fullImprintProbabilityText = diff >= TimeSpan.FromHours(2) ? "Great! :D" : diff >= TimeSpan.FromHours(1) ? "Good! :)" : diff >= TimeSpan.Zero ? "Okay. :|" : diff <= TimeSpan.FromHours(-1) ? "Bad.. :(" : diff <= TimeSpan.FromHours(-2) ? "Lousy... :(" : "Not gonna happen... :(";
                    fullImprintProbabilityText = $" and your chance of 100% imprint is {fullImprintProbabilityText}";
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine($"**Your {species} will be fully grown at {maturationFinishedWhen.ToStringWithRelativeDay()}" + (fullImprintProbabilityText != null ? fullImprintProbabilityText : "!") + "**");
            sb.AppendLine($"{cuddlesGiven} out of {cuddlesTotal} cuddles given with {cuddlesRemaining} remaining.");
            await e.Channel.SendMessage(sb.ToString().Trim('\r', '\n'));

            if (noChart)
            {
                return;
            }

            var series = data.Select(x => new DataPoint
            {
                XValue = x.cuddle / (double)cuddlesTotal,
                YValues = x.dates.Select(y => y.ToOADate()).ToArray(),
                Color = System.Drawing.Color.FromArgb(new int[] { (int)Math.Round((255f * 2) * (1 - (x.cuddle / (double)cuddlesTotal))), 255 }.Min(), new int[] { (int)Math.Round((255.0f * 2) * (x.cuddle / (double)cuddlesTotal)), 255 }.Min(), 0)
                //, Label = $"{x.cuddle:N0}/{cuddlesTotal:N0}"
            }).ToArray();

            //plot chart
            try
            {
                //using (var font = new Font("Arial", 8f))
                //{
                using (var ch = new Chart { Width = 800, Height = 400, AntiAliasing = AntiAliasingStyles.All, TextAntiAliasingQuality = TextAntiAliasingQuality.High })
                {
                    var area = new ChartArea();
                    area.AxisX.LabelStyle.Format = "{P0}";
                    area.AxisX.Maximum = 1.05;
                    area.AxisX.Interval = 0.05;
                    area.AxisX.LabelStyle.IsEndLabelVisible = false;
                    area.AxisY.LabelStyle.Format = "yyyy-MM-dd HH:mm";
                    area.AxisY.IntervalType = DateTimeIntervalType.Auto;
                    //area.AxisY.IntervalOffset = 4;
                    area.AxisY.Minimum = nextImprintWhen.AddSeconds(-3600).ToOADate();
                    area.AxisY.Maximum = new[]
                    {
                                maturationFinishedWhen,
                                nextImprintWhen.AddSeconds((cuddlesRemaining - 1) * (14400 * _config.ArkMultipliers.CuddleIntervalMultiplier))
                            }.Max().AddSeconds(3600 * _config.ArkMultipliers.CuddleIntervalMultiplier).ToOADate();
                    area.AxisX.MajorGrid.LineColor = area.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;

                    //area.AxisX.LabelStyle.Enabled = false;
                    ch.ChartAreas.Add(area);

                    var s = new Series
                    {
                        ChartType = SeriesChartType.RangeBar,
                        YValueType = ChartValueType.DateTime
                        //, Font = font
                    };
                    //s.SetCustomProperty("PixelPointWidth", "15");

                    foreach (var point in series) s.Points.Add(point);

                    var stripline = new StripLine
                    {
                        Interval = 0,
                        IntervalOffset = maturationFinishedWhen.ToOADate(),
                        StripWidth = 1,
                        BackHatchStyle = ChartHatchStyle.WideDownwardDiagonal,
                        BackColor = System.Drawing.Color.FromArgb(125, System.Drawing.Color.Silver) //System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red)
                        ,
                        Text = "Fully Grown"
                    };
                    area.AxisY.StripLines.Add(stripline);

                    ch.Series.Add(s);
                    var filepath = Path.Combine(_config.TempFileOutputDirPath, "chart.jpg");
                    ch.SaveImage(filepath, ChartImageFormat.Jpeg);

                    await e.Channel.SendFile(filepath);
                }
                //}
            }
            catch { /* ignore exceptions */  }
        }
    }
}
