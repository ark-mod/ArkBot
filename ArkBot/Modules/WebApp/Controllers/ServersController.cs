using ArkBot.Ark;
using ArkBot.Modules.Application;
using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.Application.Services;
using ArkBot.Modules.Database;
using ArkBot.Modules.Discord;
using ArkBot.Modules.WebApp.Model;
using ArkBot.Utils.Extensions;
using ArkBot.Utils.Helpers;
using ArkSavegameToolkitNet.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArkBot.Modules.WebApp.Controllers
{
    [AccessControl("pages", "home")]
    public class ServersController : BaseApiController
    {
        private EfDatabaseContextFactory _databaseContextFactory;
        private ArkContextManager _contextManager;
        private DiscordManager _discordManager;
        private IArkServerService _serverService;
        private ArkBotAnonymizeData _anonymizeData;

        public ServersController(
            IConfig config,
            EfDatabaseContextFactory databaseContextFactory,
            ArkContextManager contextManager,
            DiscordManager discordManager,
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
            var user = WebApiHelper.GetUser(HttpContext, _config);

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
                    user.Roles = _config?.WebApp?.AccessControl?.SelectMany(x => x.Value.Values)
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

                    var tamedDinosCount = context.TamedCreatures?.Count();
                    var uploadedDinosCount = context.CloudCreatures?.Count();
                    var wildDinosCount = context.WildCreatures?.Count();
                    var structuresCount = context.Structures?.Count();
                    var totalPlayers = context.Players?.Count();
                    var totalTribes = context.Tribes?.Count();
                    var serverStarted = _serverService.GetServerStartTime(context.Config.Key);

                    var nextUpdate = context.ApproxTimeUntilNextUpdate;
                    var nextUpdateTmp = nextUpdate?.ToStringCustom();
                    var nextUpdateString = nextUpdate.HasValue ? !string.IsNullOrWhiteSpace(nextUpdateTmp) ? $"~{nextUpdateTmp}" : "waiting for new update ..." : null;
                    var lastUpdate = context.LastUpdate;
                    var lastUpdateString = lastUpdate.ToStringWithRelativeDay();

                    var anonymizedServer = anonymize ? _anonymizeData.GetServer(context.Config.Key) : null;
                    var serverStatusViewModel = new ServerStatusViewModel
                    {
                        Key = anonymizedServer?.Key ?? context.Config.Key,
                        Name = anonymizedServer?.Name ?? context.Data.ServerInfo?.ServerName,
                        Address = anonymizedServer?.Address ?? context.Config.DisplayAddress ?? context.Data.ServerInfo?.Address,
                        Version = context.Data.ServerInfo != null ? $"{context.Data.ServerInfo.MajorVersion}.{context.Data.ServerInfo.MinorVersion}" : null,
                        OnlinePlayerMax = context.Data.ServerInfo?.MaxPlayers ?? 0,
                        MapName = context.Data.ServerInfo?.MapName,
                        InGameTime = context.Data.ServerInfo?.DayNumber.ToString(),
                        TamedCreatureCount = tamedDinosCount ?? 0,
                        CloudCreatureCount = uploadedDinosCount ?? 0,
                        WildCreatureCount = wildDinosCount ?? 0,
                        StructureCount = structuresCount ?? 0,
                        TribeCount = totalTribes ?? 0,
                        LastUpdate = lastUpdateString,
                        NextUpdate = nextUpdateString,
                        ServerStarted = serverStarted
                    };

                    result.Servers.Add(serverStatusViewModel);
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
            if (config.WebApp.AccessControl != null)
            {
                foreach (var fg in config.WebApp.AccessControl)
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
