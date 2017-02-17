using ArkBot.Data;
using ArkBot.Database;
using ArkBot.Extensions;
using ArkBot.Helpers;
using ArkBot.OpenID;
using Discord;
using Discord.Commands;
using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using Newtonsoft.Json;
using QueryMaster.GameServer;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using static System.FormattableString;

namespace ArkBot
{
    public class ArkDiscordBot : IDisposable
    {
        private const string _databaseConnectionString = "type=embedded;storesdirectory=.\\Database;storename=Default";
        private DiscordClient _discord;
        private ArkContext _context;
        private BarebonesSteamOpenId _openId;
        private UrlshortenerService _urlShortenerService;
        private Config _config;
        private IProgress<string> _progress;

        private const string _openidresponsetemplatePath = @"Resources\openidresponse.html";

        public ArkDiscordBot(Config config, IProgress<string> progress)
        {
            _config = config;
            _progress = progress;
            _context = new ArkContext(_config, _progress);

            var options = new SteamOpenIdOptions
            {
                ListenPrefixes = new[] { _config.SteamOpenIdRelyingServiceListenPrefix },
                RedirectUri = _config.SteamOpenIdRedirectUri,
            };
            _openId = new BarebonesSteamOpenId(options, 
                new Func<bool, ulong, ulong, Task<string>>(async (success, steamId, discordId) =>
                {
                    var razorConfig = new TemplateServiceConfiguration
                    {
                        DisableTempFileLocking = true,
                        CachingProvider = new DefaultCachingProvider(t => { })
                    };

                    using (var service = RazorEngineService.Create(razorConfig))
                    {
                        var html = await FileHelper.ReadAllTextTaskAsync(_openidresponsetemplatePath);
                        return service.RunCompile(html, _openidresponsetemplatePath, null, new { Success = success, botName = _config.BotName, botUrl = _config.BotUrl });
                    }
                }));
            _openId.SteamOpenIdCallback += _openId_SteamOpenIdCallback;

            _urlShortenerService = new UrlshortenerService(new BaseClientService.Initializer()
            {
                ApiKey = _config.GoogleApiKey,
                ApplicationName = _config.BotName,
            });

            _discord = new DiscordClient(x =>
           {
               x.LogLevel = LogSeverity.Info;
               x.LogHandler += Log;
               x.AppName = _config.BotName;
               x.AppUrl = !string.IsNullOrWhiteSpace(_config.BotUrl) ? _config.BotUrl : null;
           });

            _discord.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            var commands = _discord.GetService<CommandService>();
            commands.CommandExecuted += Commands_CommandExecuted;
            commands.CommandErrored += Commands_CommandErrored;
            commands.CreateCommand("command")
                .Alias("commands")
                .Parameter("name", ParameterType.Optional)
                .Do(ListCommands);
            commands.CreateCommand("status")
                .Alias("serverstatus")
                .Do(ServerStatus);
            commands.CreateCommand("playerlist")
                .Alias("playerslist")
                .Do(PlayerList);
            commands.CreateCommand("findtame")
                .Alias("findtames", "findpet", "findpets")
                .Parameter("name", ParameterType.Required)
                .Parameter("optional", ParameterType.Multiple)
                .Do(FindTame);
            commands.CreateCommand("mydinos")
                .Alias("mytames", "mypets")
                .Do(MyDinos);
            commands.CreateCommand("mykibbles")
                .Alias("mykibble", "myeggs")
                .Do(MyKibbles);
            commands.CreateCommand("myresources")
                .Alias("mystuff", "myitems")
                .Do(MyResources);
            commands.CreateCommand("stats")
                .Parameter("optional", ParameterType.Multiple)
                .Do(Stats);
            commands.CreateCommand("linksteam")
                .Do(LinkSteam);
            commands.CreateCommand("unlinksteam")
                .Do(UnlinkSteam);
            commands.CreateCommand("whoami")
                //.AddCheck((cmd, usr, ch) =>
                //{
                //    return ch.IsPrivate;
                //})
                .Do(WhoAmI);
            if (_config.Debug)
            {
                commands.CreateCommand("imprintcheck")
                    .Parameter("species", ParameterType.Required)
                    .Parameter("imprintPercentage", ParameterType.Required)
                    .Parameter("currentWeight", ParameterType.Required)
                    .Parameter("maxWeight", ParameterType.Required)
                    .Parameter("timeUntilNextImprint", ParameterType.Multiple)
                    .Do(ImprintCheck);
            }
        }

        private async void _openId_SteamOpenIdCallback(object sender, SteamOpenIdCallbackEventArgs e)
        {
            var ch = await _discord.CreatePrivateChannel(e.DiscordUserId);
            if (ch == null) return;

            if (e.Successful)
            {
                var player = new
                {
                    RealName = (string)null,
                    PersonaName = (string)null
                };
                try
                {
                    using (var wc = new WebClient())
                    {
                        var data = await wc.DownloadStringTaskAsync($@"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_config.SteamApiKey}&steamids={e.SteamId}");
                        var response = JsonConvert.DeserializeAnonymousType(data, new { response = new { players = new[] { player } } });
                        player = response?.response?.players?.FirstOrDefault();
                    }
                }
                catch { /* ignore exceptions */ }

                //QueryMaster.Steam.GetPlayerSummariesResponsePlayer player = null;
                //await Task.Factory.StartNew(() =>
                //{
                //    try
                //    {
                //        //this results in an exception (but it is easy enough to query by ourselves)
                //        var query = new QueryMaster.Steam.SteamQuery(_config.SteamApiKey);
                //        var result = query?.ISteamUser.GetPlayerSummaries(new[] { e.SteamId });
                //        if (result == null || !result.IsSuccess) return;

                //        player = result.ParsedResponse.Players.FirstOrDefault();
                //    }
                //    catch { /* ignore exceptions */}
                //});

                using (var ctx = new DatabaseContext(_databaseConnectionString))
                {
                    ctx.Users.AddOrUpdate(new Database.User
                    {
                        DiscordId = e.DiscordUserId,
                        SteamId = e.SteamId,
                        RealName = player?.RealName,
                        SteamDisplayName = player?.PersonaName
                    });
                    ctx.SaveChanges();
                }
                await ch.SendMessage($"Your Discord user is now linked with your Steam ID! :)");
            }
            else
            {
                await ch.SendMessage($"Something went wrong during the linking process. Please try again later!");
            }
        }

        private async Task ImprintCheck(CommandEventArgs e)
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

            if(noChart)
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

        private async Task WhoAmI(CommandEventArgs e)
        {
            using (var ctx = new DatabaseContext(_databaseConnectionString))
            {
                var user = ctx.Users.FirstOrDefault(x => x.DiscordId == e.User.Id);
                if (user == null)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, your existence is a mystery to us! :(");
                }
                else
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, I will send you a private message with everything we know about you!");

                    var sb = new StringBuilder();
                    sb.AppendLine($"**This is what we know about you:**");
                    sb.AppendLine($"● **Discord ID:** {user.DiscordId}");
                    sb.AppendLine($"● **Steam ID:** {user.SteamId}");
                    if (user.SteamDisplayName != null) sb.AppendLine($"● **Steam nick:** {user.SteamDisplayName}");
                    if (user.RealName != null) sb.AppendLine($"● **Real name:** {user.RealName}");

                    foreach (var msg in sb.ToString().Partition(2000))
                    {
                        await e.User.PrivateChannel.SendMessage(msg.Trim('\r', '\n'));
                    }
                }
            }
        }

        private async Task UnlinkSteam(CommandEventArgs e)
        {
            using (var ctx = new DatabaseContext(_databaseConnectionString))
            {
                var user = ctx.Users.FirstOrDefault(x => x.DiscordId == e.User.Id);
                if (user == null)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, your user is not linked with Steam.");
                }
                else
                {
                    ctx.DeleteObject(user);
                    ctx.SaveChanges();
                    await e.Channel.SendMessage($"<@{e.User.Id}>, your user is no longer linked with Steam.");
                }
            }
        }

        private async Task LinkSteam(CommandEventArgs e)
        {
            using (var ctx = new DatabaseContext(_databaseConnectionString))
            {
                if(ctx.Users.FirstOrDefault(x => x.DiscordId == e.User.Id) != null)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, your user is already linked with Steam. If you wish to remove this link use the command **!unlinksteam**.");
                    return;
                }
            }

            await e.Channel.SendMessage($"<@{e.User.Id}>, I will send you a private message with instructions on how to proceed with linking your Discord user with Steam!");
            var state = await _openId.LinkWithSteamTaskAsync(e.User.Id);
            var sb = new StringBuilder();
            sb.AppendLine($"**Proceed to link your Discord user with your Steam ID by following this link:**");
            sb.AppendLine($"{await ShortenUrl(state.StartUrl)}");
            await e.User.PrivateChannel.SendMessage(sb.ToString().Trim('\r', '\n'));
        }

        private async Task ListCommands(CommandEventArgs e)
        {
            var name = e.GetArg("name")?.TrimStart('!').ToLower();
            var sb = new StringBuilder();
            if (string.IsNullOrWhiteSpace(name))
            {
                sb.AppendLine($"**List of commands** (for usage examples type **!commands** <***name of command***>");
                sb.AppendLine($"● **!findtame** <***name*** (minimum length 2)> [<option (***exact/species***)>] [***tribe <name>***] [***owner <name>***] [***skip <number>***]");
                sb.AppendLine($"● **!playerlist**");
                sb.AppendLine($"● **!stats** [***tribe <name>***] [***player <name>***] [***skip <number>***]");
                sb.AppendLine($"● **!status**");
                sb.AppendLine($"● **!linksteam**");
                sb.AppendLine($"● **!unlinksteam**");
                sb.AppendLine($"● **!whoami**");
                sb.AppendLine($"● **!mydinos**");
                sb.AppendLine($"● **!mykibbles**");
                sb.AppendLine($"● **!myresources**");
            }
            else
            {
                sb.AppendLine($"**Example usage of !{name}**");
                switch (name)
                {
                    case "findtame":
                    case "findtames":
                    case "findpet":
                    case "findpets":
                        sb.AppendLine($"● **!findtame lina**: Looks for a tame using a ***partial*** name ***'lina'***");
                        sb.AppendLine($"● **!findtame lars exact**: Looks for a tame using an ***exact*** name ***'lars'***");
                        sb.AppendLine($"● **!findtame doedicurus species**: Looks for any tame of the ***species 'doedicurus'***");
                        sb.AppendLine($"● **!findtame lina owner nils**: Looks for a tame using a partial name ***'lina'*** belonging to the ***player 'nils'***");
                        sb.AppendLine($"● **!findtame lina tribe epic**: Looks for a tame using a partial name ***'lina'*** belonging to the ***tribe 'epic'***");
                        break;
                    case "mydinos":
                    case "mypets":
                    case "mytames":
                        sb.AppendLine($"● **!mydinos**: Summary of the food status of your personal- and tribe dinos");
                        break;
                    case "mykibbles":
                    case "mykibble":
                    case "myeggs":
                        sb.AppendLine($"● **!mykibbles**: Listing of your kibbles and eggs");
                        break;
                    case "myresources":
                    case "myitems":
                    case "mystuff":
                        sb.AppendLine($"● **!myresources**: Listing of your current resources");
                        break;
                    case "status":
                    case "serverstatus":
                        sb.AppendLine($"● **!status**: Current server status");
                        break;
                    case "playerlist":
                    case "playerslist":
                        sb.AppendLine($"● **!playerlist**: List of players currently online");
                        break;
                    case "stats":
                        sb.AppendLine($"● **!stats**: Statistics for the top 10 tribes by tamed dino count");
                        sb.AppendLine($"● **!stats tribe epic**: Statistics for the ***tribe 'epic'***");
                        sb.AppendLine($"● **!stats player nils**: Statistics for the ***player 'nils'***");
                        break;
                    case "linksteam":
                        sb.AppendLine($"● **!linksteam**: Link your Discord user with your Steam ID");
                        break;
                    case "unlinksteam":
                        sb.AppendLine($"● **!unlinksteam**: Unlink your Discord user from your Steam ID");
                        break;
                    case "whoami":
                        sb.AppendLine($"● **!whoami**: Find out what we know about you");
                        break;
                    default:
                        sb.Clear();
                        sb.AppendLine($"**The specified command does not exist!**");
                        break;
                }
            }
            foreach (var msg in sb.ToString().Partition(2000))
            {
                await e.Channel.SendMessage(msg.Trim('\r', '\n'));
            }
        }

        private async Task<Player> GetCurrentPlayerOrSendErrorMessage(CommandEventArgs e)
        {
            using (var ctx = new DatabaseContext(_databaseConnectionString))
            {
                var user = ctx.Users.FirstOrDefault(x => x.DiscordId == e.User.Id);
                if (user == null)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, this command can only be used after you link your Discord user with your Steam ID using **!linksteam**.");
                    return null;
                }

                var player = _context.Players.FirstOrDefault(x => x.SteamId != null && x.SteamId.Equals(user.SteamId.ToString()));
                if (player == null)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, we have no record of you playing in the last month.");
                    return null;
                }

                return player;
            }
        }

        private async Task MyKibbles(CommandEventArgs e)
        {
            var player = await GetCurrentPlayerOrSendErrorMessage(e);
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

            if(results.Length <= 0)
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

            await SendPartitioned(e.Channel, sb.ToString());
        }

        private async Task MyResources(CommandEventArgs e)
        {
            var player = await GetCurrentPlayerOrSendErrorMessage(e);
            if (player == null) return;

            var inv = (player.Inventory ?? new EntityNameWithCount[] { });
            if (player.TribeId.HasValue) inv = inv.Concat(_context.Tribes.FirstOrDefault(x => x.Id.HasValue && x.Id == player.TribeId.Value)?.Items ?? new EntityNameWithCount[] { }).ToArray();

            var includedResources = new[]
            {
                "Hide", "Thatch", "Cementing Paste", "Fiber", "Narcotic", "Spoiled Meat", "Raw Meat",
                "Wood", "Chitin", "Flint", "Silica Pearls", "Metal Ingot", "Obsidian", "Stone",
                "Keratin", "Cooked Meat Jerky", "Oil", "Prime Meat Jerky", "Pelt", "Crystal",
                "Narcoberry", "Mejoberry", "Stimberry",
                "Amarberry", "Azulberry", "Tintoberry"
            };

            var combined = new Dictionary<string, string[]> {
                { "Chitin",  new [] { "Chitin", "Keratin", "Chitin/Keratin" } },
                { "Editable berries",  new [] { "Amarberry", "Azulberry", "Tintoberry", "Mejoberry" } }
            };

            var resources = inv.Where(x => includedResources.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
                .GroupBy(x => x.Name)
                .Select(x => new EntityNameWithCount { Name = x.Key, Count = x.Sum(y => y.Count) })
                .ToList();

            var n = combined.Select(c => new EntityNameWithCount { Name = c.Key, Count = resources.Where(x => c.Value.Contains(x.Name, StringComparer.OrdinalIgnoreCase)).Sum(x => x.Count) }).ToArray();
            var remove = combined.SelectMany(y => y.Value).Except(new[] { "Mejoberry" }).ToArray();
            resources.RemoveAll(x => remove.Contains(x.Name));

            resources = resources.Concat(n).OrderBy(x => x.Name).ToList();

            if (resources.Count <= 0)
            {
                await e.Channel.SendMessage($"<@{e.User.Id}>, {(player.TribeId.HasValue ? "your tribe have" : "you have")} no resources! :(");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"**{(player.TribeId.HasValue ? "Your tribe have" : "You have")} these major resources**");

            sb.AppendLine("```");
            sb.AppendLine(FixedWidthTableHelper.ToString(resources, x => x
                .For(y => y.Name, "Type")
                .For(y => y.Count, null, 1, "N0")));
            sb.AppendLine("```");

            await SendPartitioned(e.Channel, sb.ToString());
        }

        private async Task MyDinos(CommandEventArgs e)
        {
            var player = await GetCurrentPlayerOrSendErrorMessage(e);
            if (player == null) return;

            var mydinos = _context.Creatures
                .Where(x => (x.PlayerId.HasValue && x.PlayerId.Value == player.Id) || (x.Team.HasValue && x.Team.Value == player.TribeId))
                .Select(x =>
                {
                    //!_context.ArkSpeciesStatsData.SpeciesStats.Any(y => y.Name.Equals(x.SpeciesName, StringComparison.OrdinalIgnoreCase)) ? _context.ArkSpeciesStatsData.SpeciesStats.Select(y => new { name = y.Name, similarity = StatisticsHelper.CompareToCharacterSequence(x.Name, x.SpeciesName.ToCharArray()) }).OrderByDescending(y => y.similarity).FirstOrDefault()?.name : null;
                    var speciesAliases = _context.SpeciesAliases?.GetAliases(x.SpeciesClass) ?? new[] { x.SpeciesName };
                    return new
                    {
                        creature = x,
                        maxFood = _context.ArkSpeciesStatsData?.GetMaxValue(
                            speciesAliases, //a list of alternative species names
                            Data.ArkSpeciesStatsData.Stat.Food,
                            x.WildLevels?.Food ?? 0,
                            x.TamedLevels?.Food ?? 0,
                            1d, //todo: taming efficiency is missing from ark-tools (?)
                            (double)(x.ImprintingQuality ?? 0m))
                    };
                })
                .ToArray();
            var foodStatus = mydinos?.Where(x => x.creature.CurrentFood.HasValue && x.maxFood.HasValue).Select(x => (double)x.creature.CurrentFood.Value / x.maxFood.Value)
                .Where(x => x <= 1d).OrderBy(x => x).ToArray();
            var starving = mydinos?.Where(x => x.creature.CurrentFood.HasValue && x.maxFood.HasValue).Select(x => new { creature = x.creature, p = (double)x.creature.CurrentFood.Value / x.maxFood.Value })
                .Where(x => x.p <= (1/3d)).OrderBy(x => x.p).ToArray(); //any dino below 1/3 food is considered to be starving
            //todo: babys are not idenftified in this code and as such are always considered to be starving
            if(foodStatus.Length <= 0)
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

            foreach (var msg in sb.ToString().Partition(2000))
            {
                await e.Channel.SendMessage(msg.Trim('\r', '\n'));
            }

            var filepath = Path.Combine(_config.TempFileOutputDirPath, "chart.jpg");
            if (AreaPlotSaveAs(foodStatus.Select((x, i) => new DataPoint(i, x)).ToArray(), filepath))
            {
                await e.Channel.SendFile(filepath);
            }
            if (File.Exists(filepath)) File.Delete(filepath);
        }

        private bool AreaPlotSaveAs(DataPoint[] series, string filepath)
        {
            try
            {
                using (var ch = new Chart { Width = 400, Height = 200 })
                {
                    var area = new ChartArea();
                    //area.Area3DStyle.Enable3D = true;
                    //area.Area3DStyle.LightStyle = LightStyle.Realistic;
                    area.AxisY.LabelStyle.Format = "{P0}";
                    area.AxisX.LabelStyle.Enabled = false;
                    ch.ChartAreas.Add(area);

                    var s = new Series
                    {
                        ChartType = SeriesChartType.SplineArea
                    };

                    foreach (var point in series) s.Points.Add(point);

                    ch.Series.Add(s);
                    ch.SaveImage(filepath, ChartImageFormat.Jpeg);

                    return true;
                }
            }
            catch { /* ignore exceptions */  }

            return false;
        }

        private async Task ServerStatus(CommandEventArgs e)
        {
            var status = await GetServerStatus();

            var sb = new StringBuilder();
            if (status == null || status.Item1 == null || status.Item2 == null)
            {
                sb.AppendLine($"**Serverstatus is currently unavailable!**");
            }
            else
            {
                var serverInfo = status.Item1;
                var serverRules = status.Item2;

                var m = new Regex(@"^(?<name>.+?)\s+-\s+\(v(?<version>\d+\.\d+)\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(serverInfo.Name);
                var name = m.Success ? m.Groups["name"].Value : serverInfo.Name;
                var version = m.Success ? m.Groups["version"] : null;
                var currentTime = serverRules.FirstOrDefault(x => x.Name == "DayTime_s")?.Value;
                var tamedDinosCount = _context.Creatures?.Count();
                var tamedDinosMax = 6000; //todo: remove hardcoded value
                var structuresCount = _context.Tribes?.SelectMany(x => x.Structures).Sum(x => x.Count);

                sb.AppendLine($"**{name}**");
                sb.AppendLine($"● **Address:** {serverInfo.Address}");
                if (version != null) sb.AppendLine($"● **Version:** {version}");
                sb.AppendLine($"● **Players:** {serverInfo.Players}/{serverInfo.MaxPlayers}");
                sb.AppendLine($"● **Map:** {serverInfo.Map}");
                if (tamedDinosCount.HasValue) sb.AppendLine($"● **Tamed dinos:** {tamedDinosCount.Value:N0}/{tamedDinosMax:N0}");
                if (structuresCount.HasValue) sb.AppendLine($"● **Structures:** {structuresCount.Value:N0}");
                if (currentTime != null) sb.AppendLine($"● **In-game time:** {currentTime}");
            }
            foreach (var msg in sb.ToString().Partition(2000))
            {
                await e.Channel.SendMessage(msg.Trim('\r', '\n'));
            }
        }

        private async Task PlayerList(CommandEventArgs e)
        {
            var status = await GetServerStatus();

            var sb = new StringBuilder();
            if (status == null || status.Item1 == null || status.Item3 == null)
            {
                sb.AppendLine($"**Playerlist is currently unavailable!**");
            }
            else
            {
                var serverInfo = status.Item1;
                var playerInfo = status.Item3;
                var players = playerInfo?.Where(x => !string.IsNullOrEmpty(x.Name)).ToArray() ?? new PlayerInfo[] { };

                var m = new Regex(@"^(?<name>.+?)\s+-\s+\(v(?<version>\d+\.\d+)\)$", RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(serverInfo.Name);
                var name = m.Success ? m.Groups["name"].Value : serverInfo.Name;

                sb.AppendLine($"**{name} ({serverInfo.Players - (playerInfo.Count - players.Length)}/{serverInfo.MaxPlayers})**");
                foreach (var player in players)
                {
                    sb.AppendLine($"● **{player.Name}** ({player.Time.ToStringCustom()})");
                }
            }
            foreach (var msg in sb.ToString().Partition(2000))
            {
                await e.Channel.SendMessage(msg.Trim('\r', '\n'));
            }
        }

        private async Task FindTame(CommandEventArgs e)
        {
            var query = e.GetArg("name");
            var speciesNames = _context.SpeciesAliases?.GetAliases(query);
            var optional = e.Args.Skip(1).ToArray();
            var matchExact = optional.Any(x => x.Equals("exact", StringComparison.OrdinalIgnoreCase));
            var matchSpecies = optional.Any(x => x.Equals("species", StringComparison.OrdinalIgnoreCase));
            var _1arg = optional.Take(optional.Length - 1).Select((o, i) => new { o = o, a = optional[i + 1] }).ToArray();
            var tribe = _1arg.FirstOrDefault(x => x.o.Equals("tribe", StringComparison.OrdinalIgnoreCase))?.a;
            var owner = _1arg.FirstOrDefault(x => x.o.Equals("owner", StringComparison.OrdinalIgnoreCase))?.a;
            int skip = 0;
            var _skip = _1arg.FirstOrDefault(x => x.o.Equals("skip", StringComparison.OrdinalIgnoreCase) && x.a != null && int.TryParse(x.a, out skip))?.a;
            var take = 10;

            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return;
            }

            var filtered = _context.Creatures?.Where(x => x.Tamed == true);

            if (tribe != null) filtered = filtered.Where(x => x.Tribe != null && x.Tribe.Equals(tribe, StringComparison.OrdinalIgnoreCase));
            if (owner != null) filtered = filtered.Where(x => x.OwnerName != null && x.OwnerName.Equals(owner, StringComparison.OrdinalIgnoreCase));

            if (matchExact) filtered = filtered?.Where(x => x.Name != null && x.Name.Equals(query, StringComparison.OrdinalIgnoreCase));
            else if (matchSpecies) filtered = filtered?.Where(x => speciesNames != null && x.SpeciesClass != null && speciesNames.Contains(x.SpeciesClass, StringComparer.OrdinalIgnoreCase));
            else filtered = filtered?.Where(x => (x.Name != null && x.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1) 
                || (speciesNames != null && x.SpeciesClass != null && speciesNames.Contains(x.SpeciesClass, StringComparer.OrdinalIgnoreCase)));

            var matches = filtered?.OrderByDescending(x => x.FullLevel ?? x.BaseLevel).ThenByDescending(x => x.Experience ?? decimal.MinValue).Skip(_skip != null ? skip : 0).Take(take).ToArray();
            var count = filtered.Count();
            var nextUpdate = _context.ApproxTimeUntilNextUpdate;
            var nextUpdateTmp = nextUpdate?.ToStringCustom();
            var nextUpdateString = (nextUpdate.HasValue ? (!string.IsNullOrWhiteSpace(nextUpdateTmp) ? $", next update in ~{nextUpdateTmp}" : ", waiting for new update ...") : "");
            var lastUpdate = _context.LastUpdate;
            var lastUpdateString = lastUpdate.ToStringWithRelativeDay();

            if (nextUpdate.HasValue) nextUpdate = TimeSpan.FromSeconds(Math.Round(nextUpdate.Value.TotalSeconds));
            if (matches == null || matches.Length < 1)
            {
                await e.Channel.SendMessage($"**No matching tamed creatures found!** (updated {lastUpdateString}{nextUpdateString})");
                if (matchSpecies && _context.Creatures != null && _context.SpeciesAliases != null && _context.ArkSpeciesStatsData?.SpeciesStats != null)
                {
                    //var allspecies = _context.Creatures.Select(x => x.SpeciesName).Distinct(StringComparer.OrdinalIgnoreCase).Where(x => !x.Equals("raft", StringComparison.OrdinalIgnoreCase)).ToArray();
                    var sequence = query.ToLower().ToCharArray();
                    var tamableSpecies = _context.ArkSpeciesStatsData.SpeciesStats.Select(x => x.Name).ToArray();
                    var similarity = _context.SpeciesAliases.Aliases.Where(x => tamableSpecies.Intersect(x, StringComparer.OrdinalIgnoreCase).Count() > 0).Select(x =>
                    {
                        var s = x.Select(y => new { key = y, s = StatisticsHelper.CompareToCharacterSequence(y, sequence) }).OrderByDescending(y => y.s).FirstOrDefault();
                        return new { key = s.key, primary = x.FirstOrDefault(), all = x, val = s.s /*s >= 0 ? s : 0*/ };
                    }).ToArray();
                    var possible = StatisticsHelper.FilterUsingStandardDeviation(similarity, x => x.val, (dist, sd) => dist >= sd * 1.5, false);
                    if (possible != null && possible.Length > 0)
                    {
                        var distances = possible.Select((x, i) => new { key = x.key, primary = x.primary, index = i, similarity = x.val, result = query.FindLowestLevenshteinWordDistanceInString(x.key) })
                            .Where(x => x.result != null)
                            .OrderBy(x => x.result.Item2).ThenBy(x => x.similarity).ToArray();
                        var best = StatisticsHelper.FilterUsingStandardDeviation(distances, x => x.result.Item2, (dist, sd) => dist <= sd, false);

                        var suggestions = best.Select(x => $"***\"{x.primary}\"***").ToArray().Join((n, l) => n == l ? " *or* " : "\u200B*,* ");
                        await e.Channel.SendMessage($"*Did you perhaps mean* {suggestions}\u200B*?*"); //\u200B
                    }
                }
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append($"**Found {count} matching tamed creatures");
                if (count > 10) sb.Append(" (showing top " + (_skip == null ? take.ToString() : $"{skip + 1}-{skip + matches.Length}") + ")");
                sb.AppendLine($"** (updated {lastUpdateString}{nextUpdateString})");
                foreach (var x in matches)
                {
                    sb.Append($"● {(!string.IsNullOrWhiteSpace(x.Name) ? $"**{x.Name}**, ***{x.SpeciesName}***" : $"**{x.SpeciesName}**")} (lvl ***{x.FullLevel ?? x.BaseLevel}***");
                    if (x.Tribe != null || x.OwnerName != null) sb.Append($" owned by ***{string.Join("/", new[] { x.Tribe, x.OwnerName }.Where(y => !string.IsNullOrWhiteSpace(y)).ToArray())}***");
                    sb.AppendLine(Invariant($") at ***{x.Latitude:N1}***, ***{x.Longitude:N1}***"));
                }

                foreach (var msg in sb.ToString().Partition(2000))
                {
                    await e.Channel.SendMessage(msg.Trim('\r', '\n'));
                }
                await SendAnnotatedMap(e.Channel, matches.Select(x => new PointF((float)x.Longitude, (float)x.Latitude)).ToArray());
            }
        }

        private async Task Stats(CommandEventArgs e)
        {
            var optional = e.Args.ToArray();
            var _1arg = optional.Take(optional.Length - 1).Select((o, i) => new { o = o, a = optional[i + 1] }).ToArray();
            var tribe = _1arg.FirstOrDefault(x => x.o.Equals("tribe", StringComparison.OrdinalIgnoreCase))?.a;
            var player = _1arg.FirstOrDefault(x => x.o.Equals("player", StringComparison.OrdinalIgnoreCase))?.a;
            int skip = 0;
            var _skip = _1arg.FirstOrDefault(x => x.o.Equals("skip", StringComparison.OrdinalIgnoreCase) && x.a != null && int.TryParse(x.a, out skip))?.a;
            var take = 10;

            var sb = new StringBuilder();
            var filtered = _context.Creatures.Where(x => x.Tamed == true);

            if (tribe != null)
            {
                filtered = filtered.Where(x => x.Tribe != null && x.Tribe.Equals(tribe, StringComparison.OrdinalIgnoreCase));
            }
            else if (player != null)
            {
                filtered = filtered.Where(x => x.OwnerName != null && x.OwnerName.Equals(player, StringComparison.OrdinalIgnoreCase));
            }

            var groups = filtered.GroupBy(x => player != null ? x.PlayerId : x.Team)
                .Select(x =>
                {
                    var species = x.GroupBy(y => y.SpeciesName)
                            .Select(y => new Tuple<string, int>(y.Key, y.Count())).OrderByDescending(y => y.Item2).ToArray();
                    var structuresCount = player != null ?
                        null /*_context.Players?.Where(y => y.Id == x.Key).SelectMany(y => y.Structures).Sum(y => y.Count)*/
                        : _context.Tribes?.Where(y => y.Id == x.Key).SelectMany(y => y.Structures).Sum(y => y.Count);
                    return new
                    {
                        key = x.Key,
                        name = player != null ? x.FirstOrDefault()?.OwnerName : x.FirstOrDefault()?.Tribe,
                        num = x.Count(),
                        species = tribe != null || player != null ? species : StatisticsHelper.FilterUsingStandardDeviation(species, z => z.Item2, (dist, sd) => dist >= sd, true),
                        structuresCount = structuresCount
                    };
                }).OrderByDescending(x => x.num).Skip(tribe == null && player == null && _skip != null ? skip : 0).Take(take).ToArray();

            if(tribe == null && player == null)
            {
                sb.AppendLine("**Statistics per tribe (showing top " + (_skip == null ? take.ToString() : $"{skip + 1}-{skip + groups.Length}") + ")**");
            }

            var rank = tribe == null && player == null && _skip != null ? skip : 0;
            foreach (var t in groups)
            {
                sb.AppendLine("**" + (tribe == null && player == null ? $"{rank + 1}. " : "") + $"{(!string.IsNullOrWhiteSpace(t.name) ? t.name : t.key.ToString())} have a total of {t.num:N0} tamed dinos" + (t.structuresCount.HasValue ? $" and {t.structuresCount:N0} structures" : "") + "**");
                if (t.species.Length > 0) sb.AppendLine((tribe == null && player == null ? "    which includes " : "") + "*" + t.species.Select(x => $"{x.Item2:N0} {x.Item1}").ToArray().Join((n, l) => n == l ? " and " : ", ") + "*");
                if (tribe == null && player == null) sb.AppendLine();
                rank++;
            }

            if (filtered.Count() <= 0)
            {
                await e.Channel.SendMessage($"**No statstics found**");
            }
            else
            {
                foreach (var msg in sb.ToString().Partition(2000))
                {
                    await e.Channel.SendMessage(msg.Trim('\r', '\n'));
                }
            }
        }

        private void Commands_CommandErrored(object sender, CommandErrorEventArgs e)
        {
            if (e == null || e.Command == null) return;
            var sb = new StringBuilder();
            sb.AppendLine($@"""!{e.Command.Text}{(e.Args.Length > 0 ? " " : "")}{string.Join(" ", e.Args)}"" command error...");
            if(e.Exception != null) sb.AppendLine($"Exception: {e.Exception.ToString()}");
            sb.AppendLine();
            _progress.Report(sb.ToString());
        }

        private void Commands_CommandExecuted(object sender, CommandEventArgs e)
        {
            if (e == null || e.Command == null) return;

            var sb = new StringBuilder();
            sb.AppendLine($@"""!{e.Command.Text}{(e.Args.Length > 0 ? " " : "")}{string.Join(" ", e.Args)}"" command successful!");
            _progress.Report(sb.ToString());
        }

        private async Task<Tuple<ServerInfo, QueryMaster.QueryMasterCollection<Rule>, QueryMaster.QueryMasterCollection<PlayerInfo>>> GetServerStatus()
        {
            var cache = MemoryCache.Default;

            var status = cache[nameof(GetServerStatus)] as Tuple<ServerInfo, QueryMaster.QueryMasterCollection<Rule>, QueryMaster.QueryMasterCollection<PlayerInfo>>;

            if (status == null)
            {
                await Task.Factory.StartNew(() =>
                {
                    using (var server = ServerQuery.GetServerInstance(QueryMaster.EngineType.Source, "85.227.28.132", 27003, throwExceptions: false, retries: 1, sendTimeout: 4000, receiveTimeout: 4000))
                    {
                        var serverInfo = server.GetInfo();
                        var serverRules = server.GetRules();
                        var playerInfo = server.GetPlayers();

                        status = new Tuple<ServerInfo, QueryMaster.QueryMasterCollection<Rule>, QueryMaster.QueryMasterCollection<PlayerInfo>>(serverInfo, serverRules, playerInfo);
                        cache.Set(nameof(GetServerStatus), status, new CacheItemPolicy { AbsoluteExpiration = DateTime.Now.AddMinutes(1) });

                        ////send rcon commands
                        //string rconPassword = "";
                        //if (server.GetControl(rconPassword))
                        //{
                        //    var result = server.Rcon.SendCommand("status");
                        //    server.Rcon.Dispose();
                        //}

                        ////listen to logs
                        //using (Logs logs = server.GetLogs(port))
                        //{
                        //    logs.Listen(x => Debug.WriteLine(x));
                        //    logs.Start();
                        //    //wait here
                        //    logs.Stop();
                        //}
                    }
                });
            }

            return status;
        }

        private async Task SendAnnotatedMap(Channel channel, PointF[] points)
        {
            //send map with locations marked
            var templatePath = @"Resources\theisland-template.png";
            if (!File.Exists(templatePath)) return;

            using (var image = Image.FromFile(templatePath))
            {
                using (var g = Graphics.FromImage(image))
                {
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.SmoothingMode = SmoothingMode.HighQuality;

                    foreach (var loc in points)
                    {
                        //var rc = new Rectangle(81, 86, 568, 552);
                        //var rcf = new RectangleF(12.1f, 7.2f, (float)(92.1 - 12.1), (float)(87.2 - 7.2)); //87.9
                        //var x = (float)(((loc.X - rcf.Left) / rcf.Width) * rc.Width + rc.Left);
                        //var y = (float)(((loc.Y - rcf.Top) / rcf.Height) * rc.Height + rc.Top);

                        var rc = new Rectangle(81, 86, 568, 552);
                        var gx = rc.Width / 8f;
                        var gy = rc.Height / 8f;
                        var x = (float)(((loc.X - 10) / 10) * gx + rc.Left);
                        var y = (float)(((loc.Y - 10) / 10) * gy + rc.Top);

                        g.FillCircle(Brushes.Magenta, x, y, 5f);
                    }

                    var je = ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == ImageFormat.Jpeg.Guid);
                    if (je == null) return;

                    var p = new EncoderParameters(1);
                    p.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 85L);

                    var path = Path.Combine(_config.TempFileOutputDirPath, $"{Guid.NewGuid()}.jpg");
                    image.Save(path, je, p);
                    await channel.SendFile(path);
                    File.Delete(path);
                }
            }
        }
        
        private async Task SendPartitioned(Channel channel, string message)
        {
            const int maxChars = 2000;
            var _rMarkdownTokenBegin = new Regex(@"```(?<key>[^\s]*)\s+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var markdownTokenAddedToPrev = (string)null;
            foreach (var msg in message.Partition(maxChars - 100))
            {
                var value = msg.Trim('\r', '\n');
                if (markdownTokenAddedToPrev != null)
                {
                    value = $"```{markdownTokenAddedToPrev}" + Environment.NewLine + value;
                    markdownTokenAddedToPrev = null;
                }
                var indices = value.IndexOfAll("```");
                if (indices.Length % 2 == 1)
                {
                    var m = _rMarkdownTokenBegin.Match(value, indices.Last(), value.Length - indices.Last());
                    markdownTokenAddedToPrev = m.Success ? m.Groups["key"].Value : "";
                    value = value + Environment.NewLine + "```";
                }
                await channel.SendMessage(value);
            }
        }

        private async Task<string> ShortenUrl(string longUrl)
        {
            var url = new Google.Apis.Urlshortener.v1.Data.Url
            {
                LongUrl = longUrl
            };
            return (await _urlShortenerService.Url.Insert(url).ExecuteAsync())?.Id;
        }

        public async Task Start(string botToken, ArkSpeciesAliases aliases = null)
        {
            await _context.Initialize(aliases);

            _progress.Report("Initialization done, connecting bot..." + Environment.NewLine);
            await _discord.Connect(botToken, TokenType.Bot);
        }

        public async Task Stop()
        {
            await _discord.Disconnect();
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _discord?.Dispose();
                    _discord = null;

                    _context?.Dispose();
                    _context = null;

                    _openId?.Dispose();
                    _openId = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ArkBot() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
