using ArkBot.Ark;
using ArkBot.Data;
using ArkBot.Helpers;
using ArkBot.Extensions;
using ArkBot.ViewModel;
using ArkBot.WebApi.Model;
using ArkSavegameToolkitNet.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using ArkBot.Configuration.Model;

namespace ArkBot.WebApi.Controllers
{
    public class PlayerController : BaseApiController
    {
        private ArkContextManager _contextManager;

        public PlayerController(ArkContextManager contextManager, IConfig config) : base(config)
        {
            _contextManager = contextManager;
        }

        /// <param name="id">steamId</param>
        /// <returns></returns>
        [AccessControl("pages", "player")]
        public PlayerViewModel Get([PlayerId] string id)
        {
            var demoMode = IsDemoMode() ? new DemoMode() : null;
            var result = new PlayerViewModel
            {
            };

            var uservm = WebApiHelper.GetUser(Request, _config);

            // access control
            var incProfile = HasFeatureAccess("player", "profile", id);
            var incProfileDetailed = HasFeatureAccess("player", "profile-detailed", id);
            var incCreatures = HasFeatureAccess("player", "creatures", id);
            var incCreaturesBaseStats = HasFeatureAccess("player", "creatures-basestats", id);
            var incCreaturesCloud = HasFeatureAccess("player", "creatures-cloud", id);
            var incCrops = HasFeatureAccess("player", "crops", id);
            var incGenerators = HasFeatureAccess("player", "generators", id);
            var incKibblesEggs = HasFeatureAccess("player", "kibbles-eggs", id);
            var incTribeLog = HasFeatureAccess("player", "tribelog", id);

            var players = _contextManager.Servers.ToDictionary(x => x.Config.Key, x => x.Players?.FirstOrDefault(y => y.SteamId.Equals(id, StringComparison.OrdinalIgnoreCase)));
            foreach (var context in _contextManager.Servers)
            {
                PlayerServerViewModel vm = null;

                var player = players[context.Config.Key];
                if (player == null)
                {
                    vm = BuildViewModelForTransferedPlayer(
                        context,
                        _config,
                        id,
                        players.Values.Where(x => x != null).Select(x => x.Id).ToArray(),
                        demoMode,
                        incProfile,
                        incProfileDetailed,
                        incCreatures,
                        incCreaturesBaseStats,
                        incCreaturesCloud,
                        incCrops,
                        incGenerators,
                        incKibblesEggs,
                        incTribeLog); //player have local profile on other server
                }
                else
                {
                    vm = BuildViewModelForPlayer(
                        context,
                        _config,
                        player,
                        demoMode,
                        incProfile, 
                        incProfileDetailed, 
                        incCreatures, 
                        incCreaturesBaseStats, 
                        incCreaturesCloud, 
                        incCrops, 
                        incGenerators, 
                        incKibblesEggs,
                        incTribeLog); //player with local profile
                }

                if (vm == null) continue;

                result.Servers.Add(context.Config.Key, vm);
                result.MapNames.Add(context.Config.Key, context.SaveState?.MapName);
            }

            foreach (var context in _contextManager.Clusters)
            {
                var cloudInventory = context.Inventories?.FirstOrDefault(x => x.SteamId.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (cloudInventory == null) continue;

                var vm = BuildClusterViewModelForPlayer(context, cloudInventory, demoMode, incCreaturesCloud);
                if (vm == null) continue;

                result.Clusters.Add(context.Config.Key, vm);
            }

            return result;
        }

        internal static PlayerServerViewModel BuildViewModelForTransferedPlayer(
            ArkServerContext context, 
            IConfig config,
            string steamId, 
            int[] playerIds,
            DemoMode demoMode,
            bool incProfile,
            bool incProfileDetailed,
            bool incCreatures, 
            bool incCreaturesBaseStats, 
            bool incCreaturesCloud, 
            bool incCrops, 
            bool incGenerators, 
            bool incKibblesEggs,
            bool incTribeLog)
        {
            if (playerIds == null || playerIds.Length == 0) return null;

            //there will be no player profile so most data cannot be set
            //a tribe where the player is a member may exist tho

            //note: potentially there could be multiple tribes with the same player, which player.Tribe protects us against. here we just select the first one which is not optimal
            var tribe = context.Tribes?.FirstOrDefault(x => playerIds.Any(y => x.MemberIds.Contains((int)y)));
            if (tribe == null) return null;
            var playerId = playerIds.First(x => tribe.MemberIds.Contains((int)x));

            var vm = new PlayerServerViewModel
            {
                ClusterKey = context.Config.Key,
                SteamId = steamId,
                TribeId = tribe.Id,
                TribeName = demoMode?.GetTribeName(tribe.Id) ?? tribe.Name,
                SavedAt = tribe.SavedAt
            };

            if (demoMode != null) vm.FakeSteamId = demoMode.GetSteamId(steamId);

            if (incCreatures) vm.Creatures.AddRange(BuildCreatureViewModelsForPlayerId(context, config, playerId, demoMode, incCreaturesBaseStats));
            if (incKibblesEggs) vm.KibblesAndEggs = BuildKibblesAndEggsViewModelsForPlayerId(context, playerId);
            if (incCrops) vm.CropPlots = BuildCropPlotViewModelsForPlayerId(context, playerId);
            if (incGenerators) vm.ElectricalGenerators = BuildElectricalGeneratorViewModelsForPlayerId(context, playerId);
            if (incTribeLog) vm.TribeLog = BuildTribeLogViewModelsForPlayerId(context, playerId, config.WebApp.TribeLogLimit, config.WebApp.TribeLogColors);

            return vm;
        }

        internal static PlayerServerViewModel BuildViewModelForPlayer(
            ArkServerContext context, 
            IConfig config,
            ArkPlayer player, 
            DemoMode demoMode, 
            bool incProfile, 
            bool incProfileDetailed,
            bool incCreatures, 
            bool incCreaturesBaseStats, 
            bool incCreaturesCloud, 
            bool incCrops, 
            bool incGenerators, 
            bool incKibblesEggs,
            bool incTribeLog)
        {
            var tribe = player.Tribe;
            var vm = new PlayerServerViewModel
            {
                ClusterKey = context.Config.Key,
                SteamId = player.SteamId,
                FakeSteamId = demoMode?.GetSteamId(player.SteamId),
                CharacterName = demoMode?.GetPlayerName(player.Id) ?? player.CharacterName,
                TribeId = player.TribeId,
                TribeName = tribe != null ? demoMode?.GetTribeName(tribe.Id) ?? tribe?.Name : null,
                SavedAt = player.SavedAt
            };

            if (incProfileDetailed)
            {
                vm.Latitude = player.Location?.Latitude;
                vm.Longitude = player.Location?.Longitude;
                vm.TopoMapX = player.Location?.TopoMapX;
                vm.TopoMapY = player.Location?.TopoMapY;

                if (!player.IsExternalPlayer)
                {
                    vm.Gender = player.Gender.ToString();
                    vm.Level = player.CharacterLevel;
                    vm.EngramPoints = player.TotalEngramPoints;
                }
            }

            if (incCreatures) vm.Creatures.AddRange(BuildCreatureViewModelsForPlayerId(context, config, player.Id, demoMode, incCreaturesBaseStats));
            if (incKibblesEggs) vm.KibblesAndEggs = BuildKibblesAndEggsViewModelsForPlayerId(context, player.Id);
            if (incCrops) vm.CropPlots = BuildCropPlotViewModelsForPlayerId(context, player.Id);
            if (incGenerators) vm.ElectricalGenerators = BuildElectricalGeneratorViewModelsForPlayerId(context, player.Id);
            if (incTribeLog) vm.TribeLog = BuildTribeLogViewModelsForPlayerId(context, player.Id, config.WebApp.TribeLogLimit, config.WebApp.TribeLogColors);

            return vm;
        }

        internal static List<TamedCreatureViewModel> BuildCreatureViewModelsForPlayerId(ArkServerContext context, IConfig config, int playerId, DemoMode demoMode, bool incBaseStats = false)
        {
            var result = new List<TamedCreatureViewModel>();
            if (context.TamedCreatures != null)
            {
                var player = context.Players?.FirstOrDefault(x => x.Id == playerId);
                var playercreatures = context.NoRafts.Where(x => x.TargetingTeam == playerId || (x.OwningPlayerId.HasValue && x.OwningPlayerId == playerId)).ToArray();
                var playercreatures_cryo = player?.Items?.OfType<ArkItemCryopod>().Where(x => x.Dino != null).Select(x => x.Dino).ToArray() ?? new ArkTamedCreature[] {};
                var tribe = player != null ? player.Tribe : context.Tribes?.FirstOrDefault(x => x.MemberIds.Contains((int)playerId));
                var tribecreatures = tribe != null ? context.NoRafts.Where(x => x.TargetingTeam == tribe.Id && !playercreatures.Any(y => y.Id == x.Id)).ToArray() : new ArkTamedCreature[] { };
                var tribecreatures_cryo = tribe?.Items?.OfType<ArkItemCryopod>().Where(x => x.Dino != null).Select(x => x.Dino).ToArray() ?? new ArkTamedCreature[] { };
                foreach (var item in playercreatures.Select(x => new { c = x, o = "player", cryo = false })
                    .Concat(playercreatures_cryo.Select(x => new { c = x, o = "player", cryo = true }))
                    .Concat(tribecreatures.Select(x => new { c = x, o = "tribe", cryo = false }))
                    .Concat(tribecreatures_cryo.Select(x => new { c = x, o = "tribe", cryo = true })))
                {

                    var currentFood = item.c.CurrentStatusValues?.Length > 4 ? item.c.CurrentStatusValues[4] : null;
                    var maxFood = item.c.BaseStats?.Length > 4 && item.c.TamedStats?.Length > 4 ?
                        ArkDataHelper.CalculateMaxStat(
                            ArkSpeciesStatsData.Stat.Food,
                            item.c.ClassName,
                            item.c.BaseStats[4],
                            item.c.TamedStats[4],
                            (decimal)(item.c.DinoImprintingQuality ?? 0f),
                            (decimal)(item.c.TamedIneffectivenessModifier ?? 0f)) : null;

                    //baby food formula: max * 0.1 + (max - (max * 0.1)) * age
                    if (maxFood.HasValue && item.c.BabyAge.HasValue) maxFood = maxFood.Value * 0.1 + (maxFood.Value - (maxFood.Value * 0.1)) * item.c.BabyAge.Value;

                    var foodStatus = currentFood.HasValue && maxFood.HasValue ? currentFood.Value / (float)maxFood.Value : (float?)null;
                    if (foodStatus.HasValue && foodStatus > 1f) foodStatus = 1f;

                    //baby fully grown
                    var babyFullyGrownTimeApprox = (DateTime?)null;
                    if (item.c.IsBaby && item.c.BabyAge.HasValue && context.SaveState.GameTime.HasValue)
                    {
                        var babyFullyGrown = ArkDataHelper.CalculateBabyFullyGrown(item.c.ClassName, item.c.BabyAge.Value, config);
                        babyFullyGrownTimeApprox = context.SaveState.GetApproxDateTimeOf(context.SaveState.GameTime.Value + babyFullyGrown);
                    }

                    var aliases = ArkSpeciesAliases.Instance.GetAliasesByClassName(item.c.ClassName);
                    var vmc = new TamedCreatureViewModel
                    {
                        Id1 = item.c.Id1,
                        Id2 = item.c.Id2,
                        Name = demoMode?.GetCreatureName(item.c.Id1, item.c.Id2, aliases?.FirstOrDefault()) ?? item.c.Name,
                        ClassName = item.c.ClassName,
                        Species = aliases?.FirstOrDefault(),
                        Aliases = aliases?.Skip(2).ToArray() ?? new string[] { }, //skip primary name and class name
                        Gender = item.c.Gender.ToString(),
                        BaseLevel = item.c.BaseLevel,
                        Level = item.c.Level,
                        BabyAge = item.c.IsBaby ? item.c.BabyAge : null,
                        Imprint = item.c.DinoImprintingQuality,
                        FoodStatus = foodStatus,
                        Latitude = item.cryo ? null : item.c.Location?.Latitude,
                        Longitude = item.cryo ? null : item.c.Location?.Longitude,
                        TopoMapX = item.cryo ? null : item.c.Location?.TopoMapX,
                        TopoMapY = item.cryo ? null : item.c.Location?.TopoMapY,
                        NextMating = !item.c.IsBaby && item.c.Gender == ArkCreatureGender.Female ? item.c.NextAllowedMatingTimeApprox : null,
                        BabyFullyGrown = babyFullyGrownTimeApprox,
                        BabyNextCuddle = item.c.BabyNextCuddleTimeApprox,
                        OwnerType = item.o,
                        InCryopod = item.cryo
                    };
                    if (incBaseStats)
                    {
                        //0: health
                        //1: stamina
                        //2: torpor
                        //3: oxygen
                        //4: food
                        //5: water
                        //6: temperature
                        //7: weight
                        //8: melee damage
                        //9: movement speed
                        //10: fortitude
                        //11: crafting speed

                        vmc.BaseStats = new CreatureBaseStatsViewModel {
                            Health = item.c.BaseStats[0],
                            Stamina = item.c.BaseStats[1],
                            Oxygen = item.c.BaseStats[3],
                            Food = item.c.BaseStats[4],
                            Weight = item.c.BaseStats[7],
                            Melee = item.c.BaseStats[8],
                            MovementSpeed = item.c.BaseStats[9]
                        };

                    }
                    result.Add(vmc);
                }
            }

            return result;
        }

        internal static PlayerClusterViewModel BuildClusterViewModelForPlayer(ArkClusterContext context, ArkCloudInventory cloudInventory, DemoMode demoMode, bool incCreaturesCloud)
        {
            var vm = new PlayerClusterViewModel();

            if (incCreaturesCloud)
            {
                foreach (var c in cloudInventory.Dinos)
                {
                    var aliases = ArkSpeciesAliases.Instance.GetAliasesByClassName(c.ClassName);
                    var vmc = new CloudCreatureViewModel
                    {
                        Name = demoMode?.GetCreatureName(c.Id1, c.Id2, aliases?.FirstOrDefault()) ?? c.Name,
                        ClassName = c.ClassName,
                        Species = aliases?.FirstOrDefault(),
                        Aliases = aliases?.Skip(2).ToArray() ?? new string[] { }, //skip primary name and class name
                        Level = c.Level
                    };
                    vm.Creatures.Add(vmc);
                }
            }

            return vm;
        }

        internal static List<KibbleAndEggViewModel> BuildKibblesAndEggsViewModelsForPlayerId(ArkServerContext context, int playerId)
        {
            var player = context.Players?.FirstOrDefault(x => x.Id == playerId);
            var tribe = player != null ? player.Tribe : context.Tribes?.FirstOrDefault(x => x.MemberIds.Contains(playerId));

            //PrimalItemConsumable_Egg_Kaprosuchus_C, PrimalItemConsumable_Egg_Kaprosuchus_Fertilized_C, PrimalItemConsumable_Egg_Wyvern_Fertilized_Lightning_C
            var _rEgg = new Regex(@"^PrimalItemConsumable_Egg_(?<name>.+?)_C$", RegexOptions.Singleline);

            //PrimalItemConsumable_Kibble_GalliEgg_C, PrimalItemConsumable_Kibble_Compy_C
            var _rKibble = new Regex(@"^PrimalItemConsumable_Kibble_(?<name>.+?)(?:Egg)?_C$", RegexOptions.IgnoreCase | RegexOptions.Singleline); 

            var inv = new[] { player?.Items, tribe?.Items }.Where(x => x != null).SelectMany(x => x).ToArray();

            var kibbles = inv.Where(x => x.ClassName.StartsWith("PrimalItemConsumable_Kibble", StringComparison.Ordinal))
                .GroupBy(x => x.ClassName)
                .Select(x =>
                {
                    //var name = _rKibble.Match(x.Key, m => m.Success ? m.Groups["name"].Value : x.Key);
                    //var aliases = ArkSpeciesAliases.Instance.GetAliases(name);
                    //return new { Name = aliases?.FirstOrDefault() ?? name, Count = x.Sum(y => y.Quantity) };

                    return new { Name = ArkItems.Instance.Data?.GetItem(x.Key)?.Name ?? x.Key, Count = x.Sum(y => y.Quantity) };
                })
                .ToArray();

            var eggs = inv.Where(x => x.ClassName.StartsWith("PrimalItemConsumable_Egg", StringComparison.Ordinal) && !x.ClassName.Contains("_Fertilized_"))
                .GroupBy(x => x.ClassName)
                .Select(x =>
                {
                    //var name = _rEgg.Match(x.Key, m => m.Success ? m.Groups["name"].Value : x.Key);
                    //var aliases = ArkSpeciesAliases.Instance.GetAliases(name);
                    //return new { Name = aliases?.FirstOrDefault() ?? name, Count = x.Sum(y => y.Quantity) };

                    return new { Name = ArkItems.Instance.Data?.GetItem(x.Key)?.Name ?? x.Key, Count = x.Sum(y => y.Quantity) };
                })
                .ToList();

            var keys = kibbles.Select(x => x.Name).Concat(eggs.Select(x => x.Name)).Distinct();

            var results = keys.Select(x =>
            {
                var k = kibbles.FirstOrDefault(y => y.Name.Equals(x));
                var e = eggs.FirstOrDefault(y => y.Name.Equals(x));
                return new KibbleAndEggViewModel
                {
                    Name = k?.Name ?? e?.Name,
                    KibbleCount = k?.Count ?? 0L,
                    EggCount = e?.Count ?? 0L
                };
            }).OrderByDescending(x => x.EggCount + x.KibbleCount).ToList();

            return results;
        }

        internal static List<CropPlotViewModel> BuildCropPlotViewModelsForPlayerId(ArkServerContext context, int playerId)
        {
            var player = context.Players?.FirstOrDefault(x => x.Id == playerId);
            var tribe = player != null ? player.Tribe : context.Tribes?.FirstOrDefault(x => x.MemberIds.Contains(playerId));

            var cropPlots = new[] { player?.Structures, tribe?.Structures }.Where(x => x != null).SelectMany(x => x).OfType<ArkStructureCropPlot>().Where(x => x.PlantedCropClassName != null).ToArray();
            
            var results = cropPlots.Select(x =>
            {
                return new CropPlotViewModel(x.Location)
                {
                    ClassName = x.ClassName,
                    //FertilizerAmount = x.FertilizerAmount ?? 0.0f,
                    FertilizerQuantity = (int)Math.Round(GetFertilizerQuantityFromItems(x.Inventory), 0),
                    WaterAmount = x.WaterAmount,
                    PlantedCropClassName = x.PlantedCropClassName,
                    PlantedCropName = ArkItems.Instance.Data?.GetItem(x.PlantedCropClassName, structuresPlusHack: true)?.Name?.Replace(" Seed", "") ?? x.PlantedCropClassName
                };
            }).OrderBy(x => x.Latitude).ThenBy(x => x.Longitude).ToList();

            return results;
        }

        private static readonly Dictionary<string, int> _fertilizerUnits = new Dictionary<string, int>
        {
            { "PrimalItemConsumable_HumanPoop", 1000 },
            { "PrimalItemConsumable_DinoPoopSmall", 3500 },
            { "PrimalItemConsumable_DinoPoopMedium", 7500 },
            { "PrimalItemConsumable_DinoPoopLarge", 15000 },
            { "PrimalItemConsumable_DinoPoopMassive_C", 35000 },
            { "PrimalItemConsumable_Fertilizer_Compost_C", 54000 }
        };

        internal static double GetFertilizerQuantityFromItems(ArkItem[] cropPlotInventory)
        {
            if (cropPlotInventory == null) return 0.0d;

            var fertilizerQuantity = 0.0d;
            foreach (var i in cropPlotInventory)
            {
                int units = 0;
                if (!_fertilizerUnits.TryGetValue(i.ClassName, out units)) continue;

                fertilizerQuantity += (i.SavedDurability ?? 0.0f) * units;
            }

            return fertilizerQuantity;
        }

        internal static List<ElectricalGeneratorViewModel> BuildElectricalGeneratorViewModelsForPlayerId(ArkServerContext context, int playerId)
        {
            if (!ArkSavegameToolkitNet.ArkToolkitSettings.Instance.ObjectTypes.TryGetValue(ArkSavegameToolkitNet.ObjectType.ItemElectricGeneratorGasoline, out var classNames))
                return new List<ElectricalGeneratorViewModel>();

            var player = context.Players?.FirstOrDefault(x => x.Id == playerId);
            var tribe = player != null ? player.Tribe : context.Tribes?.FirstOrDefault(x => x.MemberIds.Contains(playerId));

            var electricalGenerators = new[] { player?.Structures, tribe?.Structures }.Where(x => x != null).SelectMany(x => x).OfType<ArkStructureElectricGenerator>().ToArray();

            var results = electricalGenerators.Select(x =>
            {
                return new ElectricalGeneratorViewModel(x.Location)
                {
                    Activated = x.Activated,
                    //FuelTime = x.FuelTime, PrimalItemResource_Gasoline_C , PrimalItemResource_Gasoline_JStacks_C
                    GasolineQuantity = (int)(x.Inventory?.Where(y => classNames.Contains(y.ClassName, StringComparer.Ordinal)).Sum(y => y.Quantity) ?? 0)
                };
            }).OrderBy(x => x.Latitude).ThenBy(x => x.Longitude).ToList();

            return results;
        }

        internal static List<TribeLogEntryViewModel> BuildTribeLogViewModelsForPlayerId(ArkServerContext context, int playerId, int? limit = null, bool logColors = false)
        {
            var player = context.Players?.FirstOrDefault(x => x.Id == playerId);
            var tribe = player != null ? player.Tribe : context.Tribes?.FirstOrDefault(x => x.MemberIds.Contains(playerId));

            var tribelogs = tribe?.Logs?.Reverse().Take(limit ?? tribe.Logs.Length).Select(x => Data.TribeLog.FromLog(x)).ToArray() ?? new TribeLog[] { };
            var results = tribelogs.Select(x =>
            {
                return new TribeLogEntryViewModel
                {
                    Day = x.Day,
                    Time = x.Time,
                    Message = logColors ? x.MessageHtml : x.MessageUnformatted
                };
            }).ToList();

            return results;
        }
    }
}
