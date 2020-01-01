using Accord.Collections;
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArkBot.WebApi.Controllers
{
    [AccessControl("pages", "admin-server")]
    public class StructuresController : BaseApiController
    {
        private static readonly Dictionary<string, bool> _trashTier = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            { "AirConditioner_C", false },
            { "Beam_Metal_C", false },
            { "Beam_Wood_SM_New_C", false },
            { "BearTrapLarge_C", false },
            { "BeeHive_PlayerOwned_C", false },
            { "BeerBarrel_C", false },
            { "Bookshelf_C", false },
            { "Cage_Wood_C", false },
            { "Campfire_C", true },
            { "Ceiling_Door_Metal_C", false },
            { "Ceiling_Door_Stone_SM_C", false },
            { "Ceiling_Door_Wood_SM_New_C", false },
            { "Ceiling_Doorway_Metal_C", false },
            { "Ceiling_Doorway_Stone_C", false },
            { "Ceiling_Doorway_Wood_SM_New_C", false },
            { "Ceiling_Metal_C", false },
            { "Ceiling_Stone_C", false },
            { "Ceiling_Wood_SM_C", false },
            { "CompostBin_C", false },
            { "CookingPot_C", false },
            { "CropPlotLarge_SM_C", false },
            { "CropPlotMedium_SM_C", false },
            { "CropPlotSmall_SM_C", false },
            { "Door_Metal_C", false },
            { "Door_Stone_C", false },
            { "Door_Wood_SM_C", false },
            { "Doorframe_Metal_C", false },
            { "Doorframe_Stone_C", false },
            { "Doorframe_Wood_SM_New_C", false },
            { "Electric_Cable_Vertical_C", false },
            { "ElectricCableDiagonal_C", false },
            { "ElectricCableIntersection_C", false },
            { "ElectricCableStraight_C", false },
            { "ElectricGenerator_C", false },
            { "ElectricJunction_C", false },
            { "ElevatorPlatform_Medium_C", false },
            { "ElevatorPlatfrom_Large_C", false },
            { "ElevatorTrack_Metal_C", false },
            { "FeedingTrough_C", false },
            { "FenceFoundation_Metal_C", false },
            { "FenceFoundation_Stone_C", true },
            { "FenceFoundation_Tek_C", false },
            { "FenceFoundation_Wood_SM_C", true },
            { "Fireplace_C", false },
            { "Flag_SM_C", false },
            { "Flag_SM_Single_C", false },
            { "Flag_SM_Spider_C", false },
            { "Floor_Metal_C", false },
            { "Floor_Stone_C", false },
            { "Floor_Tek_C", false },
            { "Floor_Wood_SM_New_C", false },
            { "Forge_C", false },
            { "Gate_Large_Metal_C", false },
            { "Gate_Metal_C", false },
            { "Gate_SM_C", false },
            { "Gate_Stone_C", false },
            { "Gate_Stone_Large_C", false },
            { "GateFrame_Large_Metal_C", false },
            { "GateFrame_Metal_C", false },
            { "GateFrame_Stone_C", false },
            { "GateFrame_Stone_Large_C", false },
            { "GateFrame_Wood_SM_C", false },
            { "Greenhouse_Ceiling_C", false },
            { "Greenhouse_Door_C", false },
            { "Greenhouse_Doorframe_C", false },
            { "Greenhouse_Roof_C", false },
            { "Greenhouse_Sloped_Wall_Left_C", false },
            { "Greenhouse_Sloped_Wall_Right_C", false },
            { "Greenhouse_Wall_C", false },
            { "Greenhouse_Window_C", false },
            { "Grill_C", false },
            { "IceBox_C", false },
            { "IndustrialCookingPot_C", false },
            { "IndustrialForge_C", false },
            { "Keypad2_C", false },
            { "Ladder_Metal_C", false },
            { "Ladder_Rope_C", false },
            { "LadderBP_C", true },
            { "LampPost_C", false },
            { "LampPostOmni_C", false },
            { "MetalRoof_SM_C", false },
            { "MetalWall_Sloped_Left_SM_C", false },
            { "MetalWall_Sloped_Right_SM_C", false },
            { "ModernBed_C", false },
            { "MortarAndPestle_C", false },
            { "MotorRaft_BP_C", false },
            { "Pillar_Metal_C", false },
            { "Pillar_Stone_C", false },
            { "Pillar_Tek_C", false },
            { "Pillar_Wood_SM_New_C", false },
            { "PoisonTrap_C", false },
            { "PreservingBin_C", false },
            { "Raft_BP_C", false },
            { "Railing_Metal_C", false },
            { "Railing_Stone_C", false },
            { "Railing_Tek_C", false },
            { "Railing_Wood_C", false },
            { "Ramp_Metal_C", false },
            { "Ramp_Tek_C", false },
            { "Ramp_Wood_SM_New_C", false },
            { "Sign_Large_Metal_C", false },
            { "Sign_Large_Wood_C", false },
            { "Sign_PaintingCanvas_C", false },
            { "Sign_Small_Metal_C", false },
            { "Sign_Small_Wood_C", false },
            { "Sign_Wall_Metal_C", false },
            { "Sign_Wall_Wood_C", false },
            { "Sign_WarMap_C", false },
            { "SimpleBed_C", false },
            { "SleepingBag_C", true },
            { "SM_MetalCeilingDoorGiant_BP_C", false },
            { "SM_MetalCeilingDoorWay_Giant_BP_C", false },
            { "SM_StoneCeilingDoorGiant_BP_C", false },
            { "SM_StoneCeilingDoorWay_Giant_BP_C", false },
            { "SM_Vessel_BP_C", false },
            { "SpikeWall_C", false },
            { "SpikeWall_Wood_C", true },
            { "StairSM_Metal_C", false },
            { "StairSM_Stone_C", false },
            { "StairSM_Tek_C", false },
            { "StairSM_Wood_C", false },
            { "StandingTorch_C", true },
            { "StoneRoof_SM_C", false },
            { "StoneWall_Sloped_Left_SM_C", false },
            { "StoneWall_Sloped_Right_SM_C", false },
            { "StorageBox_AnvilBench_C", false },
            { "StorageBox_ChemBench_C", false },
            { "StorageBox_Fabricator_C", false },
            { "StorageBox_Huge_C", false },
            { "StorageBox_IndustrialGrinder_C", false },
            { "StorageBox_Large_C", true },
            { "StorageBox_Small_C", true },
            { "StorageBox_TekReplicator_C", false },
            { "Structure_TrainingDummy_C", false },
            { "StructureBP_Gravestone_C", false },
            { "StructureBP_Toilet_C", false },
            { "StructureBP_Wardrums_C", false },
            { "StructureBP_WoodBench_C", false },
            { "StructureBP_WoodChair_C", false },
            { "StructureTurretBallistaBaseBP_C", false },
            { "StructureTurretBaseBP_C", false },
            { "StructureTurretPlant_C", false },
            { "TekRoof_SM_C", false },
            { "TekWall_Sloped_Left_SM_C", false },
            { "TekWall_Sloped_Right_SM_C", false },
            { "Thatch_Ceiling_C", true },
            { "Thatch_Door_C", true },
            { "Thatch_Doorframe_C", true },
            { "Thatch_Floor_C", true },
            { "Thatch_Wall_Small_C", true },
            { "ThatchRoof_SM_C", true },
            { "ThatchWall_Sloped_Left_SM_C", true },
            { "ThatchWall_Sloped_Right_SM_C", true },
            { "TransGPSCharge_C", false },
            { "TreePlatform_Metal_SM_C", false },
            { "TreePlatform_Wood_SM_C", false },
            { "TreeSapTap_SM_C", false },
            { "TrophyBaseBP_C", false },
            { "TrophyWallBP_C", false },
            { "Wall_Metal_C", false },
            { "Wall_Stone_C", false },
            { "Wall_Tek_C", false },
            { "Wall_Wood_Small_SM_New_C", false },
            { "WallTorch_C", false },
            { "WaterPipe_Metal_Intake_C", true },
            { "WaterPipe_Metal_Intersection_C", true },
            { "WaterPipe_Metal_Straight_C", true },
            { "WaterPipe_Metal_Up_C", true },
            { "WaterPipe_Metal_Vertical_C", true },
            { "WaterPipe_Stone_Intake_C", true },
            { "WaterPipe_Stone_Intersection_C", true },
            { "WaterPipe_Stone_Straight_C", true },
            { "WaterPipe_Stone_Up_C", true },
            { "WaterPipe_Stone_Vertical_C", true },
            { "WaterTank_Metal_C", false },
            { "WaterTankBaseBP_C", false },
            { "WaterTap_C", true },
            { "WaterTap_Metal_C", true },
            { "Window_Metal_BP_C", false },
            { "Window_Stone_BP_C", false },
            { "Window_Wood_BP_C", false },
            { "WindowWall_Metal_C", false },
            { "WindowWall_Stone_C", false },
            { "WindowWall_Wood_SM_New_C", false },
            { "WindTrubine_C", false },
            { "WoodRoof_SM_C", false },
            { "WoodTable_C", false },
            { "WoodWall_Sloped_Left_SM_C", false },
            { "WoodWall_Sloped_Right_SM_C", false }
        };

        private const float _coordRadius = 3.0f;
        private ArkContextManager _contextManager;
        private ISavedState _savedState;

        public StructuresController(ISavedState savedState, ArkContextManager contextManager, IConfig config) : base(config)
        {
            _savedState = savedState;
            _contextManager = contextManager;
        }

        [AccessControl("admin-server", "structures")]
        public StructuresViewModel Get(string id)
        {
            var context = _contextManager.GetServer(id);
            if (context == null) return null;

            var demoMode = IsDemoMode() ? new DemoMode() : null;
            var result = new StructuresViewModel
            {
                MapName = context.SaveState?.MapName
            };

            var ids = new ConcurrentDictionary<string, int>();
            var types = new ConcurrentDictionary<string, StructureTypeViewModel>(StringComparer.OrdinalIgnoreCase);
            var owners = new ConcurrentDictionary<int, StructureOwnerViewModel>();

            var addStructureToArea = new Action<Area, ArkStructure, MinMaxCoords>((area, x, minmax) =>
            {
                //todo: this method may call the update callback even when the key ends up already being in the dict
                var type = types.GetOrAdd(x.ClassName, (key) =>
                {
                    return new StructureTypeViewModel(new Lazy<int>(() => ids.AddOrUpdate("typeId", 0, (key2, value) => value + 1)))
                    {
                        ClassName = x.ClassName,
                        Name = x.ClassName //todo: we do not have names for structures yet
                    };
                });
                type.Id = type._generateId.Value;

                bool isCrapTier = false;
                if (_trashTier.TryGetValue(type.ClassName, out isCrapTier) && isCrapTier) area.TrashTierCount += 1;
                area.Structures.Add(Tuple.Create(type.Id, x));

                var loc = x.Location;
                if (minmax.MinY == null || loc.Y < minmax.MinY.Y) minmax.MinY = loc;
                if (minmax.MaxY == null || loc.Y > minmax.MaxY.Y) minmax.MaxY = loc;
                if (minmax.MinX == null || loc.X < minmax.MinX.X) minmax.MinX = loc;
                if (minmax.MaxX == null || loc.X > minmax.MaxX.X) minmax.MaxX = loc;
                if (minmax.MinZ == null || loc.Z < minmax.MinZ.Z) minmax.MinZ = loc;
                if (minmax.MaxZ == null || loc.Z > minmax.MaxZ.Z) minmax.MaxZ = loc;
            });

            if (context.Structures != null)
            {
                // make fake structure objects out of rafts to include them in the clustering
                var rafts = context.Rafts?.Select(x => new ArkStructure
                {
                    ClassName = x.ClassName,
                    Location = x.Location,
                    OwnerName = x.OwningPlayerName ?? x.TribeName,
                    OwningPlayerId = x.OwningPlayerId,
                    TargetingTeam = x.TargetingTeam
                }).ToArray();

                if (rafts == null)
                    rafts = new List<ArkStructure>().ToArray();

                var structureAreas = context.Structures.Concat(rafts).Where(x => (x.TargetingTeam.HasValue || x.OwningPlayerId.HasValue) && x.Location?.Latitude != null && x.Location?.Longitude != null)
                    .GroupBy(x => x.TargetingTeam ?? x.OwningPlayerId ?? 0)
                    .AsParallel()
                    .SelectMany(x =>
                {
                    var first = x.First();
                    var arkOwnerId = first.TargetingTeam ?? first.OwningPlayerId ?? 0;
                    var isTribe = first.TargetingTeam.HasValue && (!first.OwningPlayerId.HasValue || first.TargetingTeam.Value != first.OwningPlayerId.Value);
                    var owner = owners.GetOrAdd(arkOwnerId, (key) =>
                    {
                        var tribe = isTribe ? context.Tribes.FirstOrDefault(y => y.Id == first.TargetingTeam.Value) : null;
                        var player = !isTribe ? context.Players.FirstOrDefault(y => y.Id == first.OwningPlayerId) : null;
                        var lastActiveTime = isTribe ? tribe?.LastActiveTime : player?.LastActiveTime;

                        //check saved player last active times for cross server activity
                        var externalLastActiveTime = (DateTime?)null;
                        if (isTribe && tribe != null && tribe.Members.Length > 0)
                        {
                            //for tribes check all last active times for member steamIds (across all servers/clusters)
                            var memberIds = tribe.Members.Select(y => y.SteamId).ToList();
                            var states = _savedState.PlayerLastActive.Where(y =>
                                y.SteamId != null && memberIds.Contains(y.SteamId, StringComparer.OrdinalIgnoreCase)).ToArray();
                            if (states?.Length > 0) externalLastActiveTime = states.Max(y => y.LastActiveTime);
                        }
                        else if (!isTribe && player != null)
                        {
                            //for players check all last active times for player steamid (across all servers/clusters)
                            var states = _savedState.PlayerLastActive.Where(y =>
                                y.SteamId != null && y.SteamId.Equals(player.SteamId, StringComparison.OrdinalIgnoreCase)).ToArray();
                            if (states?.Length > 0) externalLastActiveTime = states.Max(y => y.LastActiveTime);
                        }

                        //set last active time to cross server time if it is more recent
                        if ((externalLastActiveTime.HasValue && !lastActiveTime.HasValue)
                            || (externalLastActiveTime.HasValue && lastActiveTime.HasValue && externalLastActiveTime.Value > lastActiveTime.Value))
                        {
                            lastActiveTime = externalLastActiveTime;
                        }

                        return new StructureOwnerViewModel(new Lazy<int>(() => ids.AddOrUpdate("ownerId", 0, (key2, value) => value + 1)))
                        {
                            OwnerId = arkOwnerId,
                            Name = demoMode != null ? isTribe ? demoMode.GetTribeName(arkOwnerId) : demoMode.GetPlayerName(arkOwnerId) : first.OwnerName,
                            Type = isTribe ? "tribe" : "player",
                            LastActiveTime = lastActiveTime,
                            CreatureCount = (isTribe ? tribe?.Creatures.Count() : player?.Creatures.Count()) ?? 0
                        };
                    });
                    owner.Id = owner._generateId.Value;

                    var areas = new List<StructureAreaViewModel>();
                    var structures = new HashSet<ArkStructure>(x);
                    var tree = KDTree.FromData(x.Select(y => new double[] { y.Location.Latitude.Value, y.Location.Longitude.Value }).ToArray(), x.ToArray());
                    do
                    {
                        var structure = structures.First();
                        structures.Remove(structure);

                        var area = new Area();
                        var minmax = new MinMaxCoords();
                        addStructureToArea(area, structure, minmax);

                        var n = 0;
                        FindNearbyStructuresRecursive(new double[] { structure.Location.Latitude.Value, structure.Location.Longitude.Value }, structures, tree, area, minmax, ref n, addStructureToArea);

                        //var minLat = area.Min(y => y.Item2.Location.Latitude.Value);
                        //var maxLat = area.Max(y => y.Item2.Location.Latitude.Value);
                        //var minLng = area.Min(y => y.Item2.Location.Longitude.Value);
                        //var maxLng = area.Max(y => y.Item2.Location.Longitude.Value);
                        var dLat = (minmax.MaxY.Latitude.Value - minmax.MinY.Latitude.Value) / 2f;
                        var dLng = (minmax.MaxX.Longitude.Value - minmax.MinX.Longitude.Value) / 2f;
                        var avgLat = minmax.MinY.Latitude.Value + dLat;
                        var avgLng = minmax.MinX.Longitude.Value + dLng;

                        var dY = (minmax.MaxY.Y - minmax.MinY.Y) / 2f;
                        var dX = (minmax.MaxX.X - minmax.MinX.X) / 2f;
                        var dZ = (minmax.MaxZ.Z - minmax.MinZ.Z) / 2f;
                        var avgY = minmax.MinY.Y + dY;
                        var avgX = minmax.MinX.X + dX;
                        var avgZ = minmax.MinZ.Z + dZ;

                        var dTopoMapY = (minmax.MaxY.TopoMapY.Value - minmax.MinY.TopoMapY.Value) / 2f;
                        var dTopoMapX = (minmax.MaxX.TopoMapX.Value - minmax.MinX.TopoMapX.Value) / 2f;
                        var avgTopoMapY = minmax.MinY.TopoMapY.Value + dTopoMapY;
                        var avgTopoMapX = minmax.MinX.TopoMapX.Value + dTopoMapX;

                        var structureGroups = area.Structures.GroupBy(y => y.Item1).Select(y => new StructureViewModel
                        {
                            TypeId = y.Key,
                            Count = y.Count()
                        }).OrderByDescending(y => y.Count).ToList();

                        areas.Add(new StructureAreaViewModel
                        {
                            OwnerId = owner.Id,
                            Structures = structureGroups,
                            StructureCount = area.Structures.Count,
                            Latitude = (float)Math.Round(avgLat, 2),
                            Longitude = (float)Math.Round(avgLng, 2),
                            Radius = (float)Math.Round(Math.Sqrt(dLat * dLat + dLng * dLng), 2),
                            TopoMapX = (float)Math.Round(avgTopoMapX, 2),
                            TopoMapY = (float)Math.Round(avgTopoMapY, 2),
                            RadiusPx = (float)Math.Round(Math.Sqrt(dTopoMapX * dTopoMapX + dTopoMapY * dTopoMapY), 2),
                            X = (float)Math.Round(avgX, 2),
                            Y = (float)Math.Round(avgY, 2),
                            Z = (float)Math.Round(avgZ, 2),
                            RadiusUu = (float)Math.Round(Math.Sqrt(dX * dX + dY * dY), 2),
                            TrashQuota = area.TrashTierCount / (float)area.Structures.Count
                        });
                    } while (structures.Count > 0);

                    owner.AreaCount = areas.Count;
                    owner.StructureCount = areas.Sum(y => y.StructureCount);

                    return areas;
                }).ToArray();

                result.Areas = structureAreas.OrderByDescending(x => x.Radius).ThenByDescending(x => x.StructureCount).ToList();
                result.Owners = owners.Values.OrderBy(x => x.Id).ToList();
                result.Types = types.Values.OrderBy(x => x.Id).ToList();
            }

            return result;
        }

        private static void FindNearbyStructuresRecursive(double[] pt, HashSet<ArkStructure> points, KDTree<ArkStructure> tree, Area area, MinMaxCoords minmax, ref int pointsRemoved, Action<Area, ArkStructure, MinMaxCoords> addStructureToArea)
        {
            var near = tree.Nearest(pt, _coordRadius).Where(y => points.Contains(y.Node.Value)).ToArray();
            foreach (var item in near)
            {
                addStructureToArea(area, item.Node.Value, minmax);
                points.Remove(item.Node.Value);
            }
            pointsRemoved += near.Length;

            if (points.Count == 0) return;
            if (pointsRemoved >= 50)
            {
                tree = KDTree.FromData(points.Select(y => new double[] { y.Location.Latitude.Value, y.Location.Longitude.Value }).ToArray(), points.ToArray());
                pointsRemoved = 0;
            }

            foreach (var item in near)
            {
                FindNearbyStructuresRecursive(item.Node.Position, points, tree, area, minmax, ref pointsRemoved, addStructureToArea);
            }
        }

        private class MinMaxCoords
        {
            public ArkLocation MinY { get; set; }
            public ArkLocation MaxY { get; set; }
            public ArkLocation MinX { get; set; }
            public ArkLocation MaxX { get; set; }
            public ArkLocation MinZ { get; set; }
            public ArkLocation MaxZ { get; set; }
        }

        private class Area
        {
            public Area()
            {
                Structures = new List<Tuple<int, ArkStructure>>();
            }

            public int TrashTierCount { get; set; }
            public List<Tuple<int, ArkStructure>> Structures { get; set; }
        }
    }
}
