using ArkBot.Ark;
using ArkBot.Configuration.Model;
using ArkBot.Database;
using ArkBot.Extensions;
using ArkBot.Helpers;
using ArkBot.ViewModel;
using ArkBot.WebApi.Model;
using ArkSavegameToolkitNet.Domain;
using Discord;
using QueryMaster.GameServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArkBot.WebApi.Controllers
{
    [AccessControl("pages", "home")]
    public class ServersController : BaseApiController
    {
        private EfDatabaseContextFactory _databaseContextFactory;
        private ArkContextManager _contextManager;
        private Discord.DiscordManager _discordManager;
        private IArkServerService _serverService;
        private ArkBotAnonymizeData _anonymizeData;

        public ServersController(
            IConfig config,
            EfDatabaseContextFactory databaseContextFactory,
            ArkContextManager contextManager,
            Discord.DiscordManager discordManager,
            IArkServerService serverService,
            ArkBotAnonymizeData anonymizeData) : base(config)
        {
            _databaseContextFactory = databaseContextFactory;
            _contextManager = contextManager;
            _discordManager = discordManager;
            _serverService = serverService;
            _anonymizeData = anonymizeData;
        }

        public async Task<ServerStatusAllViewModel> Get()
        {
            var demoMode = IsDemoMode() ? new DemoMode() : null;

            var anonymize = _config.AnonymizeWebApiData;
            var user = WebApiHelper.GetUser(Request, _config);

            if (anonymize)
            {
                var serverContext = _contextManager?.Servers?.FirstOrDefault();
                var player = serverContext?.Players?.Where(x =>
                        x.Tribe != null && x.Tribe.Creatures != null && x.Tribe.Structures != null
                        && x.Tribe.Creatures.Any(y => y.IsBaby)
                        && x.Tribe.Structures.OfType<ArkStructureCropPlot>().Any()
                        && x.Tribe.Structures.OfType<ArkStructureElectricGenerator>().Any()
                    ).OrderByDescending(x => x.Creatures.Length).FirstOrDefault();

                if (player == null)
                {
                    user = null;
                }
                else
                {
                    user.Name = player.Name;
                    user.SteamId = player.SteamId;
                    user.Roles = _config?.AccessControl?.SelectMany(x => x.Value.Values)
                        .SelectMany(x => x)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(x => x)
                        .ToArray();
                }
            }

            var result = new ServerStatusAllViewModel { User = user, AccessControl = BuildViewModelForAccessControl(_config) };
            if (_contextManager == null)
            {
                return result;
            }

            if (_contextManager.Servers != null)
            {
                foreach (var context in _contextManager.Servers)
                {
                    var serverContext = _contextManager.GetServer(context.Config.Key);
                    var status = serverContext.Steam.GetServerStatusCached();
                    //if (status == null || status.Item1 == null || status.Item2 == null)
                    //{
                    //    //Server status is currently unavailable
                    //}
                    //else
                    //{
                    var info = status?.Item1;
                    var rules = status?.Item2;
                    var playerinfos = status?.Item3;

                    var matched = info != null ? new Regex(@"^(?<name>.+?)\s+-\s+\(v(?<version>\d+\.\d+)\)?$", RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(info.Name) : null;
                    var name = matched?.Success == true ? matched.Groups["name"].Value : (info?.Name ?? context.Config.Key);
                    var version = matched?.Success == true ? matched.Groups["version"] : null;
                    var currentTime = rules?.FirstOrDefault(x => x.Name == "DayTime_s")?.Value;
                    var tamedDinosCount = context.TamedCreatures?.Count();
                    var uploadedDinosCount = context.CloudCreatures?.Count();
                    var wildDinosCount = context.WildCreatures?.Count();
                    //var tamedDinosMax = 6000; //todo: remove hardcoded value
                    var structuresCount = context.Structures?.Count();
                    var totalPlayers = context.Players?.Count();
                    var totalTribes = context.Tribes?.Count();
                    var serverStarted = _serverService.GetServerStartTime(context.Config.Key);

                    var nextUpdate = context.ApproxTimeUntilNextUpdate;
                    var nextUpdateTmp = nextUpdate?.ToStringCustom();
                    var nextUpdateString = (nextUpdate.HasValue ? (!string.IsNullOrWhiteSpace(nextUpdateTmp) ? $"~{nextUpdateTmp}" : "waiting for new update ...") : null);
                    var lastUpdate = context.LastUpdate;
                    var lastUpdateString = lastUpdate.ToStringWithRelativeDay();

                    var anonymizedServer = anonymize ? _anonymizeData.GetServer(context.Config.Key) : null;
                    var serverStatusViewModel = new ServerStatusViewModel
                    {
                        Key = anonymizedServer?.Key ?? context.Config.Key,
                        Name = anonymizedServer?.Name ?? name,
                        Address = anonymizedServer?.Address ?? context.Config.DisplayAddress ?? info?.Address,
                        Version = version?.ToString(),
                        OnlinePlayerCount = info?.Players ?? 0,
                        OnlinePlayerMax = info?.MaxPlayers ?? 0,
                        MapName = info?.Map,
                        InGameTime = currentTime,
                        TamedCreatureCount = tamedDinosCount ?? 0,
                        CloudCreatureCount = uploadedDinosCount ?? 0,
                        WildCreatureCount = wildDinosCount ?? 0,
                        StructureCount = structuresCount ?? 0,
                        PlayerCount = totalPlayers ?? 0,
                        TribeCount = totalTribes ?? 0,
                        LastUpdate = lastUpdateString,
                        NextUpdate = nextUpdateString,
                        ServerStarted = serverStarted
                    };

                    serverStatusViewModel.OnlinePlayers = new List<OnlinePlayerViewModel>();
                    if (HasFeatureAccess("home", "online"))
                    {
                        var onlineplayers = playerinfos?.Where(x => !string.IsNullOrEmpty(x.Name)).ToArray() ?? new PlayerInfo[] { };
                        if (anonymize)
                        {
                            if (context.Players != null)
                            {
                                int n = 0;
                                foreach (var player in context.Players.OrderByDescending(x => x.LastActiveTime).Take(onlineplayers.Length))
                                {
                                    if (player == null)
                                    {
                                        continue;
                                    }

                                    var timeSpan = onlineplayers[n] != null ? onlineplayers[n].Time : TimeSpan.Zero;
                                    serverStatusViewModel.OnlinePlayers.Add(new OnlinePlayerViewModel
                                    {
                                        SteamName = player.Name,
                                        CharacterName = player.CharacterName,
                                        TribeName = player.Tribe?.Name,
                                        DiscordName = null,
                                        TimeOnline = timeSpan.ToStringCustom(),
                                        TimeOnlineSeconds = (int)Math.Round(timeSpan.TotalSeconds)
                                    });

                                    n++;
                                }
                            }
                        }
                        else
                        {
                            using (var db = _databaseContextFactory.Create())
                            {
                                // Names of online players (steam does not provide ids)
                                var onlineplayerNames = onlineplayers.Select(x => x.Name).ToArray();

                                // Get the player data for each name (null when no matching name or when multiple players share the same name)
                                var onlineplayerData = context.Players != null ? (from k in onlineplayerNames join p in context.Players on k equals p.Name into grp select grp.Count() == 1 ? grp.ElementAt(0) : null).ToArray() : new ArkPlayer[] { };

                                // Parse all steam ids
                                var parsedSteamIds = onlineplayerData.Select(x => { long steamId; return x?.SteamId != null ? long.TryParse(x.SteamId, out steamId) ? steamId : (long?)null : null; }).ToArray();

                                // Get the player data for each name
                                var databaseUsers = (from k in parsedSteamIds join u in db.Users on k equals u?.SteamId into grp select grp.FirstOrDefault()).ToArray();

                                // Get the discord users
                                var discordUsers = databaseUsers.Select(x => x != null ? _discordManager.GetDiscordUserNameById((ulong)x.DiscordId) : null).ToArray();

                                int n = 0;
                                foreach (var player in onlineplayers)
                                {
                                    var extra = onlineplayerData.Length > n ? new { player = onlineplayerData[n], user = databaseUsers[n], discordName = discordUsers[n] } : null;
                                    var demoPlayerName = demoMode?.GetPlayerName();

                                    var timeSpan = player != null ? player.Time : TimeSpan.Zero;
                                    serverStatusViewModel.OnlinePlayers.Add(new OnlinePlayerViewModel
                                    {
                                        SteamName = demoPlayerName ?? player.Name,
                                        CharacterName = demoPlayerName ?? extra?.player?.CharacterName,
                                        TribeName = demoMode?.GetTribeName() ?? extra?.player?.Tribe?.Name,
                                        DiscordName = demoMode != null ? null : extra?.discordName,
                                        TimeOnline = timeSpan.ToStringCustom(),
                                        TimeOnlineSeconds = (int)Math.Round(timeSpan.TotalSeconds)
                                    });

                                    n++;
                                }
                            }
                        }
                    }


                    result.Servers.Add(serverStatusViewModel);
                    //}
                }
            }


            if (_contextManager.Clusters != null)
            {
                foreach (var context in _contextManager.Clusters)
                {
                    var cc = new ClusterStatusViewModel
                    {
                        Key = context?.Config?.Key,
                        ServerKeys = _contextManager.Servers
                            .Where(x => x.Config != null && x.Config.ClusterKey != null && x.Config.ClusterKey.Equals(context.Config.Key, StringComparison.OrdinalIgnoreCase))
                            .Select(x => x.Config.Key).ToArray()
                    };
                    result.Clusters.Add(cc);
                }
            }

            return result;
        }

        private static AccessControlViewModel BuildViewModelForAccessControl(IConfig config)
        {
            var ac = new AccessControlViewModel { };
            if (config.AccessControl != null)
            {
                foreach (var fg in config.AccessControl)
                {
                    var acfg = new Dictionary<string, List<string>>();
                    ac[fg.Key] = acfg;

                    if (fg.Value == null) continue;

                    foreach (var rf in fg.Value)
                    {
                        acfg[rf.Key] = rf.Value;
                    }
                }
            }

            return ac;
        }
    }
}
