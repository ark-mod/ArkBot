using ArkBot.Ark;
using ArkBot.Configuration.Model;
using ArkBot.Data;
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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace ArkBot.WebApi.Controllers
{
    [AccessControl("pages", "admin-server")]
    public class AdministerController : BaseApiController
    {
        private ArkContextManager _contextManager;

        private Regex _rDestroyedStructureCount = new Regex(@"^Destroyed (?<num>\d+) structures", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private Regex _rFertilizedEggCount = new Regex(@"^Found (?<num>\d+) fertilized eggs on the map", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private Regex _rFertilizedEgg = new Regex(@"^(?<bp>\w+) \(lvl (?<level>\d+)\): Spoiling in (?<time>.+)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        public AdministerController(ArkContextManager contextManager, IConfig config) : base(config)
        {
            _contextManager = contextManager;
        }

        [HttpGet]
        [AccessControl("admin-server", "structures-rcon")]
        public async Task<HttpResponseMessage> SaveWorld(string id)
        {
            var serverContext = _contextManager.GetServer(id);
            if (serverContext == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Server instance key not found!");

            var result = await serverContext.Steam.SendRconCommand($"SaveWorld");
            if (result == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Timeout while waiting for command response...");

            return Request.CreateResponse(HttpStatusCode.OK, new AdministerResponseViewModel
            {
                Message = "World saved! Please wait for server update (new data)..."
            });
        }

        [HttpGet]
        [AccessControl("admin-server", "fertilized-eggs")]
        public async Task<HttpResponseMessage> DroppedEggsList(string id)
        {
            var serverContext = _contextManager.GetServer(id);
            if (serverContext == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Server instance key not found!");

            var result = await serverContext.Steam.SendRconCommand($"DroppedEggs list");
            if (result == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Timeout while waiting for command response...");

            if (result.TrimEnd('\n').Equals("There are no fertilized eggs on the map."))
            {
                return Request.CreateResponse(HttpStatusCode.OK, new FertilizedEggsResponseViewModel
                {
                    Message = result?.TrimEnd('\n'),
                    FertilizedEggsCount = 0,
                    SpoiledFertilizedEggsCount = 0
                });
            }

            var m = _rFertilizedEggCount.Match(result);

            var eggList = result.Split('\n').Skip(1).Where(a => !string.IsNullOrWhiteSpace(a));

            var fertilizedEggList = new List<FertilizedEggViewModel>();

            foreach (var egg in eggList)
            {
                var eggInfo = _rFertilizedEgg.Match(egg);

                fertilizedEggList.Add(new FertilizedEggViewModel
                {
                    CharacterBP = eggInfo.Success ? eggInfo.Groups["bp"].Value : null,
                    SpoilTime = eggInfo.Success ? eggInfo.Groups["time"].Value : null,
                    EggLevel = eggInfo.Success ? int.Parse(eggInfo.Groups["level"].Value) : (int?)null,
                    Dino = eggInfo.Success ? ArkSpeciesAliases.Instance.GetAliases(eggInfo.Groups["bp"].Value).FirstOrDefault() : null
                });
            }


            return Request.CreateResponse(HttpStatusCode.OK, new FertilizedEggsResponseViewModel
            {
                Message = result?.TrimEnd('\n'),
                FertilizedEggsCount = m.Success ? int.Parse(m.Groups["num"].Value) : (int?)null,
                FertilizedEggList = fertilizedEggList,
                SpoiledFertilizedEggsCount = 0
            });
        }


        [HttpGet]
        [AccessControl("admin-server", "structures-rcon")]
        public async Task<HttpResponseMessage> DestroyAllStructuresForTeamId(string id, string teamId)
        {
            var serverContext = _contextManager.GetServer(id);
            if (serverContext == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Server instance key not found!");

            var result = await serverContext.Steam.SendRconCommand($"DestroyAllStructuresForTeamId {teamId}");
            if (result == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Timeout while waiting for command response...");

            var m = _rDestroyedStructureCount.Match(result);
            return Request.CreateResponse(HttpStatusCode.OK, new AdministerResponseViewModel
            {
                Message = result?.TrimEnd('\n'),
                DestroyedStructureCount = m.Success ? int.Parse(m.Groups["num"].Value) : (int?)null
            });
        }

        [HttpGet]
        [AccessControl("admin-server", "structures-rcon")]
        public async Task<HttpResponseMessage> DestroyStructuresForTeamIdAtPosition(string id, string teamId, float x, float y, float radius, int rafts)
        {
            var serverContext = _contextManager.GetServer(id);
            if (serverContext == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Server instance key not found!");

            var result = await serverContext.Steam.SendRconCommand($"DestroyStructuresForTeamIdAtPosition {teamId} {x} {y} {radius} {rafts}");
            if (result == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Timeout while waiting for command response...");

            var m = _rDestroyedStructureCount.Match(result);
            return Request.CreateResponse(HttpStatusCode.OK, new AdministerResponseViewModel
            {
                Message = result?.TrimEnd('\n'),
                DestroyedStructureCount = m.Success ? int.Parse(m.Groups["num"].Value) : (int?)null
            });
        }

        [HttpGet]
        [AccessControl("admin-server", "structures-rcon")]
        public async Task<HttpResponseMessage> DestroyDinosForTeamId(string id, string teamId)
        {
            var serverContext = _contextManager.GetServer(id);
            if (serverContext == null) return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Server instance key not found!");

            var result = await serverContext.Steam.SendRconCommand($"DestroyDinosForTeamId {teamId}");
            if (result == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Timeout while waiting for command response...");

            return Request.CreateResponse(HttpStatusCode.OK, new AdministerResponseViewModel
            {
                Message = result?.TrimEnd('\n')
            });
        }
    }
}
