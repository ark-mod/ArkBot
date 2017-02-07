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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
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
            _openId = new BarebonesSteamOpenId(options);
            _openId.SteamOpenIdCallback += _openId_SteamOpenIdCallback;

            _urlShortenerService = new UrlshortenerService(new BaseClientService.Initializer()
            {
                ApiKey = _config.GoogleApiKey,
                ApplicationName = "ARKSverige Discord Bot",
            });

            _discord = new DiscordClient(x =>
           {
               x.LogLevel = LogSeverity.Info;
               x.LogHandler += Log;
               x.AppName = "ARKSverige Discord Bot";
               x.AppUrl = "http://www.arksverige.se/";
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

        private async Task MyDinos(CommandEventArgs e)
        {
            using (var ctx = new DatabaseContext(_databaseConnectionString))
            {
                var user = ctx.Users.FirstOrDefault(x => x.DiscordId == e.User.Id);
                if (user == null)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, this command can only be used after you link your Discord user with your Steam ID using **!linksteam**.");
                    return;
                }

                var player = _context.Players.FirstOrDefault(x => x.SteamId.Equals(user.SteamId.ToString()));
                if (player == null)
                {
                    await e.Channel.SendMessage($"<@{e.User.Id}>, we have no record of you playing in the last month.");
                    return;
                }

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
                var min = foodStatus.Min();
                var avg = foodStatus.Average();
                var max = foodStatus.Max();

                var stateFun = min <= 0.25 ? "starving... :(" : min <= 0.5 ? "hungry... :|" : min <= 0.75 ? "feeling satisfied. :)" : "feeling happy! :D";

                var sb = new StringBuilder();
                sb.AppendLine($"**Your dinos are {stateFun}**");
                sb.AppendLine($"{min:P0} ≤ {avg:P0} ≤ {max:P0}");

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
            else if (matchSpecies) filtered = filtered?.Where(x => x.SpeciesName != null && x.SpeciesName.Equals(query, StringComparison.OrdinalIgnoreCase));
            else filtered = filtered?.Where(x => x.Name != null && x.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) != -1);

            var matches = filtered?.OrderByDescending(x => x.FullLevel ?? x.BaseLevel).ThenByDescending(x => x.Experience ?? decimal.MinValue).Skip(_skip != null ? skip : 0).Take(take).ToArray();
            var count = filtered.Count();
            var nextUpdate = _context.ApproxTimeUntilNextUpdate;
            var nextUpdateString = (nextUpdate.HasValue ? (nextUpdate.Value.TotalSeconds >= 0 ? $", next update in ~{nextUpdate.Value.ToStringCustom()}" : ", waiting for new update ...") : "");
            var lastUpdate = _context.LastUpdate;
            var isToday = DateTime.Today == lastUpdate.Date;
            var isYesterday = DateTime.Today.AddDays(-1).Date == lastUpdate.Date;
            var lastUpdateString = isToday ? $"{lastUpdate:'today at' HH:mm}" : isYesterday ? $"{lastUpdate:'yesterday at' HH:mm}" : $"{lastUpdate:yyyy-MM-dd HH:mm}";

            if (nextUpdate.HasValue) nextUpdate = TimeSpan.FromSeconds(Math.Round(nextUpdate.Value.TotalSeconds));
            if (matches == null || matches.Length < 1)
            {
                await e.Channel.SendMessage($"**No matching tamed creatures found!** (updated {lastUpdateString}{nextUpdateString})");
                if (matchSpecies && _context.Creatures != null)
                {

                    var allspecies = _context.Creatures.Select(x => x.SpeciesName).Distinct(StringComparer.OrdinalIgnoreCase).Where(x => !x.Equals("raft", StringComparison.OrdinalIgnoreCase)).ToArray();
                    var sequence = query.ToLower().ToCharArray();
                    var similarity = allspecies.Select(x =>
                    {
                        var s = StatisticsHelper.CompareToCharacterSequence(x, sequence);
                        return new { key = x, val = s /*s >= 0 ? s : 0*/ };
                    }).ToArray();
                    var possible = StatisticsHelper.FilterUsingStandardDeviation(similarity, x => x.val, (dist, sd) => dist >= sd * 1.5, false);
                    if (possible != null && possible.Length > 0)
                    {
                        var distances = possible.Select((x, i) => new { key = x.key, index = i, similarity = x.val, result = query.FindLowestLevenshteinWordDistanceInString(x.key) })
                            .Where(x => x.result != null)
                            .OrderBy(x => x.result.Item2).ThenBy(x => x.similarity).ToArray();
                        var best = StatisticsHelper.FilterUsingStandardDeviation(distances, x => x.result.Item2, (dist, sd) => dist <= sd, false);

                        var suggestions = best.Select(x => $"***{x.key}***").ToArray().Join((n, l) => n == l ? " *or* " : "\u200B*,* ");
                        await e.Channel.SendMessage($"*Did you perhaps mean \"*\u200B{suggestions}\u200B*\"?*");
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
