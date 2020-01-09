using ArkBot.Ark;
using ArkBot.Configuration.Model;
using ArkBot.Data;
using ArkBot.WebApi.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Controllers
{
    [AccessControl("pages", "admin-server")]
    public class AdministerController : BaseApiController
    {
        private ArkContextManager _contextManager;

        private Regex _rDestroyedStructureCount = new Regex(@"^Destroyed (?<num>\d+) structures", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private Regex _rFertilizedEggCount = new Regex(@"^Found (?<num>\d+) fertilized eggs on the map", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private Regex _rFertilizedEgg = new Regex(@"^(?<bp>\w+) \(lvl (?<level>\d+)\): Spoiling in (?<time>.+)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private Regex _rSpoiledEgg = new Regex(@"^(?<bp>\w+) \(lvl (?<level>\d+)\): Spoiled", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private Regex _rFertilizedEggPlayerDropped = new Regex(@"^(?<bp>\w+) \(lvl (?<level>\d+), dropped by '(?<player>\w+) - Lvl (?<playerLevel>\d+)'\): Spoiling in (?<time>.+)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private Regex _rSpoiledEggPlayerDropped = new Regex(@"^(?<bp>\w+) \(lvl (?<level>\d+), dropped by '(?<player>\w+) - Lvl (?<playerLevel>\d+)'\): Spoiled", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        public AdministerController(ArkContextManager contextManager, IConfig config) : base(config)
        {
            _contextManager = contextManager;
        }

        [HttpGet("saveworld/{id}")]
        [AccessControl("admin-server", "structures-rcon")]
        public async Task<IActionResult> SaveWorld(string id)
        {
            var serverContext = _contextManager.GetServer(id);
            if (serverContext == null) return BadRequest("Server instance key not found!");

            var result = await serverContext.Steam.SendRconCommand($"SaveWorld");
            if (result == null) return InternalServerError("Timeout while waiting for command response...");

            return Ok(new AdministerResponseViewModel
            {
                Message = "World saved! Please wait for server update (new data)..."
            });
        }

        [HttpGet("destroyspoiledeggs/{id}")]
        [AccessControl("admin-server", "structures-rcon")]
        public async Task<IActionResult> DestroySpoiledEggs(string id)
        {
            var serverContext = _contextManager.GetServer(id);
            if (serverContext == null) return BadRequest("Server instance key not found!");

            var result = await serverContext.Steam.SendRconCommand($"DroppedEggs destroy_spoiled_including_dropped_by_player");
            if (result == null) return InternalServerError("Timeout while waiting for command response...");

            return Ok(new AdministerResponseViewModel
            {
                Message = "All spoiled eggs destroyed!"
            });
        }

        [HttpGet("destroyalleggs/{id}")]
        [AccessControl("admin-server", "structures-rcon")]
        public async Task<IActionResult> DestroyAllEggs(string id)
        {
            var serverContext = _contextManager.GetServer(id);
            if (serverContext == null) return BadRequest("Server instance key not found!");

            var result = await serverContext.Steam.SendRconCommand($"DroppedEggs destroy_all_including_dropped_by_player");
            if (result == null) return InternalServerError("Timeout while waiting for command response...");

            return Ok(new AdministerResponseViewModel
            {
                Message = "All eggs destroyed!"
            });
        }

        [HttpGet("droppedeggslist/{id}")]
        [AccessControl("admin-server", "fertilized-eggs")]
        public async Task<IActionResult> DroppedEggsList(string id)
        {
            var serverContext = _contextManager.GetServer(id);
            if (serverContext == null) return BadRequest("Server instance key not found!");

            var result = await serverContext.Steam.SendRconCommand($"DroppedEggs list");
            if (result == null) return InternalServerError("Timeout while waiting for command response...");

            if (result.TrimEnd('\n').Equals("There are no fertilized eggs on the map."))
            {
                return Ok(new FertilizedEggsResponseViewModel
                {
                    Message = result?.TrimEnd('\n'),
                    FertilizedEggsCount = 0,
                    SpoiledEggsCount = 0
                });
            }

            var eggList = result.Split('\n').Skip(1).Where(a => !string.IsNullOrWhiteSpace(a));
            var fertilizedEggList = new List<FertilizedEggViewModel>();
            var spoiledEggList = new List<FertilizedEggViewModel>();

            foreach (var egg in eggList)
            {
                var fertilized = _rFertilizedEgg.Match(egg);
                var spoiled = _rSpoiledEgg.Match(egg);
                var fertilizedPlayerDropped = _rFertilizedEggPlayerDropped.Match(egg);
                var spoiledPlayerDropped = _rSpoiledEggPlayerDropped.Match(egg);

                if (fertilized.Success)
                {
                    fertilizedEggList.Add(new FertilizedEggViewModel
                    {
                        CharacterBP = fertilized.Success ? fertilized.Groups["bp"].Value : null,
                        SpoilTime = fertilized.Success ? fertilized.Groups["time"].Value : null,
                        EggLevel = fertilized.Success ? int.Parse(fertilized.Groups["level"].Value) : (int?)null,
                        Dino = fertilized.Success ? ArkSpeciesAliases.Instance.GetAliases(fertilized.Groups["bp"].Value).FirstOrDefault() : null
                    });
                }
                else if (fertilizedPlayerDropped.Success)
                {
                    fertilizedEggList.Add(new FertilizedEggViewModel
                    {
                        CharacterBP = fertilizedPlayerDropped.Success ? fertilizedPlayerDropped.Groups["bp"].Value : null,
                        SpoilTime = fertilizedPlayerDropped.Success ? fertilizedPlayerDropped.Groups["time"].Value : null,
                        EggLevel = fertilizedPlayerDropped.Success ? int.Parse(fertilizedPlayerDropped.Groups["level"].Value) : (int?)null,
                        Dino = fertilizedPlayerDropped.Success ? ArkSpeciesAliases.Instance.GetAliases(fertilizedPlayerDropped.Groups["bp"].Value).FirstOrDefault() : null,
                        DroppedBy = fertilizedPlayerDropped.Success ? fertilizedPlayerDropped.Groups["player"].Value + " - " + fertilizedPlayerDropped.Groups["playerLevel"].Value : null,
                        DroppedBySteamId = serverContext.Players.FirstOrDefault(a => a.CharacterName.Equals(fertilizedPlayerDropped.Groups["player"].Value))?.SteamId ?? null
                    });
                }
                else if (spoiled.Success)
                {
                    spoiledEggList.Add(new FertilizedEggViewModel
                    {
                        CharacterBP = spoiled.Success ? spoiled.Groups["bp"].Value : null,
                        Dino = spoiled.Success ? ArkSpeciesAliases.Instance.GetAliases(spoiled.Groups["bp"].Value).FirstOrDefault() : null,
                        EggLevel = spoiled.Success ? int.Parse(spoiled.Groups["level"].Value) : (int?)null
                    });
                }
                else if (spoiledPlayerDropped.Success)
                {
                    spoiledEggList.Add(new FertilizedEggViewModel
                    {
                        CharacterBP = spoiledPlayerDropped.Success ? spoiledPlayerDropped.Groups["bp"].Value : null,
                        Dino = spoiledPlayerDropped.Success ? ArkSpeciesAliases.Instance.GetAliases(spoiledPlayerDropped.Groups["bp"].Value).FirstOrDefault() : null,
                        EggLevel = spoiledPlayerDropped.Success ? int.Parse(spoiledPlayerDropped.Groups["level"].Value) : (int?)null,
                        DroppedBy = spoiledPlayerDropped.Success ? spoiledPlayerDropped.Groups["player"].Value + " - " + spoiledPlayerDropped.Groups["playerLevel"].Value : null,
                        DroppedBySteamId = serverContext.Players.FirstOrDefault(a => a.CharacterName.Equals(spoiledPlayerDropped.Groups["player"].Value))?.SteamId ?? null
                    });
                }
            }


            return Ok(new FertilizedEggsResponseViewModel
            {
                Message = result?.TrimEnd('\n'),
                FertilizedEggsCount = fertilizedEggList.Count,
                FertilizedEggList = fertilizedEggList,
                SpoiledEggList = spoiledEggList,
                SpoiledEggsCount = spoiledEggList.Count,
            });
        }


        [HttpGet("destroyallstructuresforteamid/{id}")]
        [AccessControl("admin-server", "structures-rcon")]
        public async Task<IActionResult> DestroyAllStructuresForTeamId(string id, string teamId)
        {
            var serverContext = _contextManager.GetServer(id);
            if (serverContext == null) return BadRequest("Server instance key not found!");

            var result = await serverContext.Steam.SendRconCommand($"DestroyAllStructuresForTeamId {teamId}");
            if (result == null) return InternalServerError("Timeout while waiting for command response...");

            var m = _rDestroyedStructureCount.Match(result);
            return Ok(new AdministerResponseViewModel
            {
                Message = result?.TrimEnd('\n'),
                DestroyedStructureCount = m.Success ? int.Parse(m.Groups["num"].Value) : (int?)null
            });
        }

        [HttpGet("destroystructuresforteamidatposition/{id}")]
        [AccessControl("admin-server", "structures-rcon")]
        public async Task<IActionResult> DestroyStructuresForTeamIdAtPosition(string id, string teamId, float x, float y, float radius, int rafts)
        {
            var serverContext = _contextManager.GetServer(id);
            if (serverContext == null) return BadRequest("Server instance key not found!");

            var result = await serverContext.Steam.SendRconCommand($"DestroyStructuresForTeamIdAtPosition {teamId} {x} {y} {radius} {rafts}");
            if (result == null) return InternalServerError("Timeout while waiting for command response...");

            var m = _rDestroyedStructureCount.Match(result);
            return Ok(new AdministerResponseViewModel
            {
                Message = result?.TrimEnd('\n'),
                DestroyedStructureCount = m.Success ? int.Parse(m.Groups["num"].Value) : (int?)null
            });
        }

        [HttpGet("destroydinosforteamid/{id}")]
        [AccessControl("admin-server", "structures-rcon")]
        public async Task<IActionResult> DestroyDinosForTeamId(string id, string teamId)
        {
            var serverContext = _contextManager.GetServer(id);
            if (serverContext == null) return BadRequest("Server instance key not found!");

            var result = await serverContext.Steam.SendRconCommand($"DestroyDinosForTeamId {teamId}");
            if (result == null) return InternalServerError("Timeout while waiting for command response...");

            return Ok(new AdministerResponseViewModel
            {
                Message = result?.TrimEnd('\n')
            });
        }
    }
}
