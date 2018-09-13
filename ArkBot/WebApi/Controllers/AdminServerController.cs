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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArkBot.WebApi.Controllers
{
    [AccessControl("pages", "admin-server")]
    public class AdminServerController : BaseApiController
    {
        private ArkContextManager _contextManager;

        public AdminServerController(ArkContextManager contextManager, IConfig config) : base(config)
        {
            _contextManager = contextManager;
        }

        public ServerViewModel Get(string id) //, int? limit)
        {
            var context = _contextManager.GetServer(id);
            if (context == null) return null;

            var demoMode = IsDemoMode() ? new DemoMode() : null;
            var result = new AdminServerViewModel
            {
            };

            var creatureCounts = context.NoRafts?.GroupBy(x => x.TargetingTeam).ToDictionary(x => x.Key, x => x.Count());
            var structureCounts = context.Structures?.Where(x => x.TargetingTeam.HasValue).GroupBy(x => x.TargetingTeam.Value).ToDictionary(x => x.Key, x => x.Count());

            if (HasFeatureAccess("admin-server", "players") && context.Players != null)
            {
                result.Players.AddRange(context.Players.Select(x =>
                {
                    int cc1 = 0, cc2 = 0, sc1 = 0, sc2 = 0;
                    creatureCounts?.TryGetValue((int)x.Id, out cc1);
                    structureCounts?.TryGetValue((int)x.Id, out sc1);
                    if (x.TribeId.HasValue)
                    {
                        creatureCounts?.TryGetValue(x.TribeId.Value, out cc2);
                        structureCounts?.TryGetValue(x.TribeId.Value, out sc2);
                    }

                    var tribe = x.TribeId != null ? context.Tribes?.FirstOrDefault(y => y.Id == x.TribeId) : null;
                    return new AdminPlayerReferenceViewModel
                    {
                        Id = x.Id,
                        SteamId = x.SteamId,
                        FakeSteamId = demoMode?.GetSteamId(x.SteamId),
                        CharacterName = demoMode?.GetPlayerName(x.Id) ?? x.CharacterName,
                        SteamName = null,
                        TribeName = tribe != null ? demoMode?.GetTribeName(tribe.Id) ?? tribe.Name : null,
                        TribeId = x.TribeId,
                        CreatureCount = cc1 + cc2,
                        StructureCount = sc1 + sc2,
                        LastActiveTime = x.LastActiveTime
                    };
                }).OrderByDescending(x => x.LastActiveTime));
            }

            if (HasFeatureAccess("admin-server", "tribes") && context.Tribes != null)
            {
                result.Tribes.AddRange(context.Tribes.Select(x =>
                {
                    int cc1 = 0, sc1 = 0;
                    creatureCounts?.TryGetValue((int)x.Id, out cc1);
                    structureCounts?.TryGetValue((int)x.Id, out sc1);
                    foreach (var m in x.MemberIds)
                    {
                        int cc2 = 0, sc2 = 0;
                        creatureCounts?.TryGetValue(m, out cc2);
                        structureCounts?.TryGetValue(m, out sc2);
                        cc1 += cc2;
                        sc1 += sc2;
                    }

                    var members = context.Players?.Where(y => x.MemberIds.Contains((int)y.Id)).ToList() ?? new List<ArkPlayer>();
                    return new AdminTribeReferenceViewModel
                    {
                        Id = x.Id,
                        Name = demoMode?.GetTribeName(x.Id) ?? x.Name,
                        MemberSteamIds = members.Select(y => y.SteamId).ToList(),
                        CreatureCount = cc1,
                        StructureCount = sc1,
                        LastActiveTime = x.LastActiveTime
                    };
                }).OrderByDescending(x => x.LastActiveTime));
            }

            return result;
        }
    }
}
