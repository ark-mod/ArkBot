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
//using System.Globalization;
//using System.Windows.Forms.DataVisualization.Charting;
//using System.IO;
//using ArkBot.Data;

//namespace ArkBot.Commands.Experimental
//{
//    public class ImprintCheckCommand : ICommand
//    {
//        public string Name => "imprintcheck";
//        public string[] Aliases => null;
//        public string Description => "Check if a 100% imprint is likely to succeed or fail with the given stats";
//        public string SyntaxHelp => null;
//        public string[] UsageExamples => null;

//        public bool DebugOnly => false;
//        public bool HideFromCommandList => true;

//        private IConfig _config;
//        private IArkContext _context;

//        public ImprintCheckCommand(IConfig config, IArkContext context)
//        {
//            _config = config;
//            _context = context;
//        }

//        public void Register(CommandBuilder command)
//        {
//            //command.Parameter("species", ParameterType.Required)
//            //    .Parameter("imprintPercentage", ParameterType.Required)
//            //    .Parameter("currentWeight", ParameterType.Required)
//            //    .Parameter("maxWeight", ParameterType.Required)
//            //    .Parameter("timeUntilNextImprint", ParameterType.Multiple);

//            command.AddCheck((a, b, c) => c.Client.Servers.Any(x => x.Roles.Any(y => y != null && y.Name.Equals(_config.DeveloperRoleName) && y.Members.Any(z => z.Id == b.Id))), null)
//                .Parameter("optional", ParameterType.Optional)
//                .Hide();
//        }

//        public void Init(Discord.DiscordClient client) { }

//        public async Task Run(CommandEventArgs e)
//        {
//            var args = CommandHelper.ParseArgs(e, new { all = false }, x =>
//                x.For(y => y.all, flag: true));
//            if (args == null)
//            {
//                await e.Channel.SendMessage(string.Join(Environment.NewLine, new string[] {
//                    $"**My logic circuits cannot process this command! I am just a bot after all... :(**",
//                    !string.IsNullOrWhiteSpace(SyntaxHelp) ? $"Help me by following this syntax: **!{Name}** {SyntaxHelp}" : null }.Where(x => x != null)));
//                return;
//            }

//            var sb = new StringBuilder();

//            var breedingStats = _context.ArkSpeciesStatsData?.SpeciesStats/*.Where(x => x.Name.Equals("Angler", StringComparison.OrdinalIgnoreCase))*/.Select(x => new { species = x, breedingAdj = x?.Breeding.GetAdjusted(_config.ArkMultipliers) });
//            if (breedingStats == null) return;

//            var rnd = new Random();
//            var data = breedingStats.Where(x => x.breedingAdj != null && x.breedingAdj.MaturationTime > 0).Select(x =>
//            {
//                var cuddlesTotal = (int)Math.Ceiling((x.breedingAdj.MaturationTime / (14400 * _config.ArkMultipliers.CuddleIntervalMultiplier)));
//                var cuddleBonus = 1d / (x.breedingAdj.MaturationTime / (14400 * _config.ArkMultipliers.CuddleIntervalMultiplier));
//                var cuddlesMinTime = TimeSpan.FromSeconds(cuddlesTotal * (10800 * _config.ArkMultipliers.CuddleIntervalMultiplier));
//                var cuddlesMaxTime = TimeSpan.FromSeconds(cuddlesTotal * (14400 * _config.ArkMultipliers.CuddleIntervalMultiplier));
//                var maturationTime = TimeSpan.FromSeconds(x.breedingAdj.MaturationTime);
//                var aliases = ArkSpeciesAliases.Instance.GetAliases(x.species.Name);
//                var name = aliases?.FirstOrDefault() ?? x.species.Name;
//                var cuddlesGuaranteed = (int)Math.Floor(x.breedingAdj.MaturationTime / (14400 * _config.ArkMultipliers.CuddleIntervalMultiplier));
//                //var chance = cuddlesMinTime >= maturationTime ? 0f : cuddlesMaxTime < maturationTime ? 1f : (maturationTime-cuddlesMinTime).TotalSeconds / (cuddlesMaxTime-cuddlesMinTime).TotalSeconds;

//                var limit = (maturationTime - cuddlesMinTime).TotalSeconds;
//                var iterations = 1000000;
//                var c = 0;
//                for (var i = 0; i < iterations; i++)
//                {
//                    var v = 0d;
//                    for (var j = 0; j < cuddlesTotal; j++) v += rnd.Next(3601);
//                    if (v < limit) c++;
//                }

//                return new
//                {
//                    Species = name,
//                    MaturationTime = maturationTime.ToStringCustom(),
//                    Cuddles = cuddlesTotal,
//                    ImprintGuaranteed = cuddlesGuaranteed == 0 ? 0d : Math.Round(cuddlesGuaranteed * cuddleBonus * 100d),
//                    CuddlesMinTime = cuddlesMinTime.ToStringCustom(),
//                    CuddlesMaxTime = cuddlesMaxTime.ToStringCustom(),
//                    ChanceOfPerfectImprint = Math.Round((c / (double)iterations) * 100, 1)
//                };
//            }).OrderBy(x => x.Species).ToArray();

//            sb.AppendLine($"**100% Imprint Chance per Species ({_config.ArkMultipliers.BabyMatureSpeedMultiplier:0.#}x) [assumes true random distribution for cuddle interval]**");
//            sb.AppendLine(@"Note: ""% Imprint Guaranteed"" is the imprint % achievable if you do not miss any imprint and do them fast but all cuddle intervals are 4h which is the worst case scenario.");
//            sb.AppendLine("```");
//            sb.AppendLine(FixedWidthTableHelper.ToString(data, x => x
//                .For(y => y.Species, "Species")
//                .For(y => y.MaturationTime, "Maturation Time", 1)
//                .For(y => y.Cuddles, "Cuddles", 1, "N0")
//                .For(y => y.ImprintGuaranteed, "% Imprint Guaranteed", 1, "#'%';-#'%';0'%'", fordefault: "-")
//                .For(y => y.CuddlesMinTime, "Cuddles Min Time", 1)
//                .For(y => y.CuddlesMaxTime, "Cuddles Max Time", 1)
//                .For(y => y.ChanceOfPerfectImprint, "% Chance", 1, "0.#'%';-0.#'%';0'%'", fordefault: "-")));
//            sb.AppendLine("```");

//            var msg = sb.ToString();
//            if (!string.IsNullOrWhiteSpace(msg)) await CommandHelper.SendPartitioned(e.Channel, sb.ToString());

//            //var species = e.GetArg("species");
//            //var speciesNames = _context.SpeciesAliases?.GetAliases(species);
//            //if (speciesNames?.Length <= 0) return;

//            //species = speciesNames.FirstOrDefault(x => x.Equals(species, StringComparison.OrdinalIgnoreCase)) ?? species;

//            //double imprintPercentage = 0d;
//            //if (double.TryParse(e.GetArg("imprintPercentage")?.Replace(",", ".").Replace("%", "").Trim() ?? "", NumberStyles.Float & ~NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out imprintPercentage))
//            //{
//            //    if (e.GetArg("imprintPercentage")?.IndexOf('%') == -1) imprintPercentage *= 100d;
//            //}
//            //else return;

//            //double currentWeight = 0d;
//            //if (!double.TryParse(e.GetArg("currentWeight")?.Replace(",", ".") ?? "", NumberStyles.Float & (NumberStyles.Float & ~NumberStyles.AllowExponent), CultureInfo.InvariantCulture, out currentWeight)) return;

//            //double maxWeight = 0d;
//            //if (!double.TryParse(e.GetArg("maxWeight")?.Replace(",", ".") ?? "", NumberStyles.Float & ~NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out maxWeight)) return;

//            //if (e.Command["timeUntilNextImprint"] == null) return;
//            //var timeUntilNextImprint = TimeSpanHelper.ParseFromString(string.Join(" ", e.Args.Skip(e.Command["timeUntilNextImprint"].Id).ToArray()));
//            //if (timeUntilNextImprint?.TotalSeconds < 0) return;

//            //var breedingStats = _context.ArkSpeciesStatsData?.SpeciesStats.FirstOrDefault(x => speciesNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))?.Breeding.GetAdjusted(_config.ArkMultipliers);
//            //if (breedingStats == null) return;

//            //var grownFraction = currentWeight / maxWeight;

//            //var imprintBonus = Math.Round(
//            //    Math.Round(imprintPercentage * breedingStats.MaturationTime / (14400 * _config.ArkMultipliers.CuddleIntervalMultiplier))
//            //    * 14400 * _config.ArkMultipliers.CuddleIntervalMultiplier / (breedingStats.MaturationTime)
//            //, 4);

//            //var now = DateTime.Now;
//            //var nextImprintWhen = now + timeUntilNextImprint.Value;
//            //var maturationFinishedWhen = now.AddSeconds(breedingStats.MaturationTime - (breedingStats.MaturationTime * grownFraction));

//            ////todo: is this correct?
//            //var cuddlesTotal = (int)Math.Ceiling((breedingStats.MaturationTime / (14400 * _config.ArkMultipliers.CuddleIntervalMultiplier)));
//            //var cuddlesGiven = (int)Math.Round((imprintBonus / 100) * cuddlesTotal);
//            //var cuddlesRemaining = cuddlesTotal - cuddlesGiven;

//            //if (cuddlesGiven >= cuddlesTotal) return;

//            //var totalSecondsSinceEpoch = new Func<DateTime, double>((dt) => dt.ToUniversalTime().Subtract(
//            //        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
//            //    ).TotalSeconds);

//            //var data = Enumerable.Range(cuddlesGiven + 1, cuddlesRemaining).Select(x => new
//            //{
//            //    cuddle = x,
//            //    dates = new[]
//            //    {
//            //        nextImprintWhen.AddSeconds(x == cuddlesGiven + 1 ?
//            //           3600
//            //            : (x - (cuddlesGiven + 1)) * (10800 * _config.ArkMultipliers.CuddleIntervalMultiplier)),
//            //        nextImprintWhen.AddSeconds((x - (cuddlesGiven + 1)) * (14400 * _config.ArkMultipliers.CuddleIntervalMultiplier))
//            //    }
//            //}).ToArray();

//            ////Debug.WriteLine($"Next imprint at {nextImprintWhen:yyyy-MM-dd HH:mm}");
//            ////Debug.WriteLine($"Fully grown at {maturationFinishedWhen:yyyy-MM-dd HH:mm}");
//            ////Debug.WriteLine($"{cuddlesGiven} of {cuddlesTotal} given ({cuddlesRemaining} remain)");
//            ////foreach (var d in data)
//            ////{
//            ////    Debug.WriteLine($"Cuddle #{d.cuddle}: {d.dates[0]:yyyy-MM-dd HH:mm} - {d.dates[1]:yyyy-MM-dd HH:mm}");
//            ////}

//            //var noChart = false;
//            //var finalCuddle = data.LastOrDefault()?.dates;
//            //string fullImprintProbabilityText = null;
//            //if (finalCuddle != null && finalCuddle.Length >= 2)
//            //{
//            //    if (finalCuddle.First() > maturationFinishedWhen)
//            //    {
//            //        fullImprintProbabilityText = " but sadly it will not reach 100% imprint... :(";
//            //        noChart = true;
//            //    }
//            //    else
//            //    {
//            //        var finalCuddleMean = finalCuddle.First().AddSeconds((finalCuddle.Last() - finalCuddle.First()).TotalSeconds / 2d);
//            //        var diff = maturationFinishedWhen - finalCuddleMean;
//            //        fullImprintProbabilityText = diff >= TimeSpan.FromHours(2) ? "Great! :D" : diff >= TimeSpan.FromHours(1) ? "Good! :)" : diff >= TimeSpan.Zero ? "Okay. :|" : diff <= TimeSpan.FromHours(-1) ? "Bad.. :(" : diff <= TimeSpan.FromHours(-2) ? "Lousy... :(" : "Not gonna happen... :(";
//            //        fullImprintProbabilityText = $" and your chance of 100% imprint is {fullImprintProbabilityText}";
//            //    }
//            //}

//            //var sb = new StringBuilder();
//            //sb.AppendLine($"**Your {species} will be fully grown at {maturationFinishedWhen.ToStringWithRelativeDay()}" + (fullImprintProbabilityText != null ? fullImprintProbabilityText : "!") + "**");
//            //sb.AppendLine($"{cuddlesGiven} out of {cuddlesTotal} cuddles given with {cuddlesRemaining} remaining.");
//            //await e.Channel.SendMessage(sb.ToString().Trim('\r', '\n'));

//            //if (noChart)
//            //{
//            //    return;
//            //}

//            //var series = data.Select(x => new DataPoint
//            //{
//            //    XValue = x.cuddle / (double)cuddlesTotal,
//            //    YValues = x.dates.Select(y => y.ToOADate()).ToArray(),
//            //    Color = System.Drawing.Color.FromArgb(new int[] { (int)Math.Round((255f * 2) * (1 - (x.cuddle / (double)cuddlesTotal))), 255 }.Min(), new int[] { (int)Math.Round((255.0f * 2) * (x.cuddle / (double)cuddlesTotal)), 255 }.Min(), 0)
//            //    //, Label = $"{x.cuddle:N0}/{cuddlesTotal:N0}"
//            //}).ToArray();

//            ////plot chart
//            //try
//            //{
//            //    //using (var font = new Font("Arial", 8f))
//            //    //{
//            //    using (var ch = new Chart { Width = 800, Height = 400, AntiAliasing = AntiAliasingStyles.All, TextAntiAliasingQuality = TextAntiAliasingQuality.High })
//            //    {
//            //        var area = new ChartArea();
//            //        area.AxisX.LabelStyle.Format = "{P0}";
//            //        area.AxisX.Maximum = 1.05;
//            //        area.AxisX.Interval = 0.05;
//            //        area.AxisX.LabelStyle.IsEndLabelVisible = false;
//            //        area.AxisY.LabelStyle.Format = "yyyy-MM-dd HH:mm";
//            //        area.AxisY.IntervalType = DateTimeIntervalType.Auto;
//            //        //area.AxisY.IntervalOffset = 4;
//            //        area.AxisY.Minimum = nextImprintWhen.AddSeconds(-3600).ToOADate();
//            //        area.AxisY.Maximum = new[]
//            //        {
//            //                    maturationFinishedWhen,
//            //                    nextImprintWhen.AddSeconds((cuddlesRemaining - 1) * (14400 * _config.ArkMultipliers.CuddleIntervalMultiplier))
//            //                }.Max().AddSeconds(3600 * _config.ArkMultipliers.CuddleIntervalMultiplier).ToOADate();
//            //        area.AxisX.MajorGrid.LineColor = area.AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;

//            //        //area.AxisX.LabelStyle.Enabled = false;
//            //        ch.ChartAreas.Add(area);

//            //        var s = new Series
//            //        {
//            //            ChartType = SeriesChartType.RangeBar,
//            //            YValueType = ChartValueType.DateTime
//            //            //, Font = font
//            //        };
//            //        //s.SetCustomProperty("PixelPointWidth", "15");

//            //        foreach (var point in series) s.Points.Add(point);

//            //        var stripline = new StripLine
//            //        {
//            //            Interval = 0,
//            //            IntervalOffset = maturationFinishedWhen.ToOADate(),
//            //            StripWidth = 1,
//            //            BackHatchStyle = ChartHatchStyle.WideDownwardDiagonal,
//            //            BackColor = System.Drawing.Color.FromArgb(125, System.Drawing.Color.Silver) //System.Drawing.Color.FromArgb(50, System.Drawing.Color.Red)
//            //            ,
//            //            Text = "Fully Grown"
//            //        };
//            //        area.AxisY.StripLines.Add(stripline);

//            //        ch.Series.Add(s);
//            //        var filepath = Path.Combine(_config.TempFileOutputDirPath, "chart.jpg");
//            //        ch.SaveImage(filepath, ChartImageFormat.Jpeg);

//            //        await e.Channel.SendFile(filepath);
//            //    }
//            //    //}
//            //}
//            //catch { /* ignore exceptions */  }
//        }
//    }
//}
