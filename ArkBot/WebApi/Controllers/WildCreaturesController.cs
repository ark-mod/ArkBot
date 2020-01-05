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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;

namespace ArkBot.WebApi.Controllers
{
    [AccessControl("pages", "server")]
    public class WildCreaturesController : BaseApiController
    {
        private ArkContextManager _contextManager;

        public static readonly string[] UntameableClassNames = new[]
        {
            "MegaCarno_Character_BP_C",
            "MegaDeathworm_Character_BP_C",
            "MegaWyvern_Character_BP_Fire_C",
            "Alpha_Leedsichthys_Character_BP_C",
            "MegaMegalodon_Character_BP_C",
            "Mosa_Character_BP_Mega_C",
            "MegaRaptor_Character_BP_C",
            "MegaRex_Character_BP_C",
            "Mega_Tusoteuthis_Character_BP_C",
            "Ammonite_Character_BP_C",
            "Bone_MegaWyvern_Character_BP_Fire_C",
            "SpiderL_Character_BP_C",
            "SpiderL_Character_BP_Easy_C",
            "SpiderL_Character_BP_Medium_C",
            "SpiderL_Character_BP_Hard_C",
            "SpiderL_Character_BP_TheCenter_C",
            "SpiderL_Character_BP_TheCenterMedium_C",
            "SpiderL_Character_BP_TheCenterHard_C",
            "Dodo_Character_BP_Bunny_C",
            "BunnyOviRaptor_Character_BP_C",
            "Cnidaria_Character_BP_C",
            "Coel_Character_BP_C",
            "Coel_Character_BP_Ocean_C",
            "Deathworm_Character_BP_C",
            "DodoWyvern_Character_BP_C",
            "Dragon_Character_BP_Boss_C",
            "Dragon_Character_BP_Boss_Easy_C",
            "Dragon_Character_BP_Boss_Medium_C",
            "Dragon_Character_BP_Boss_Hard_C",
            "Dragonfly_Character_BP_C",
            "Euryp_Character_C",
            "PlayerPawnTest_Male_C",
            "PlayerPawnTest_Female_C",
            "Jugbug_Character_BaseBP_C",
            "Jugbug_Oil_Character_BP_C",
            "Jugbug_Water_Character_BP_C",
            "Leech_Character_C",
            "Leech_character_Diseased_C",
            "Leedsichthys_Character_BP_C",
            "Manticore_Character_BP_C",
            "Manticore_Character_BP_Easy_C",
            "Manticore_Character_BP_Medium_C",
            "Manticore_Character_BP_Hard_C",
            "Gorilla_Character_BP_C",
            "Gorilla_Character_BP_Easy_C",
            "Gorilla_Character_BP_Medium_C",
            "Gorilla_Character_BP_Hard_C",
            "Gorilla_Character_BP_TheCenter_C",
            "Gorilla_Character_BP_TheCenter_Medium_C",
            "Gorilla_Character_BP_TheCenter_Hard_C",
            "Piranha_Character_BP_C",
            "Salmon_Character_BP_C",
            "Bone_Sauropod_Character_BP_C",
            "Bone_MegaCarno_Character_BP_C",
            "Bone_Gigant_Character_BP_C",
            "Bone_Jerboa_Character_BP_C",
            "Bone_Quetz_Character_BP_C",
            "Bone_MegaRaptor_Character_BP_C",
            "Bone_MegaRex_Character_BP_C",
            "Bone_Stego_Character_BP_C",
            "Bone_Trike_Character_BP_C",
            "BoaFrill_Character_BP_C",
            "Ant_Character_BP_C",
            "FlyingAnt_Character_BP_C",
            "Trilobite_Character_C",
            "Turkey_Character_BP_C",
            "Wyvern_Character_BP_Base_C",
            "Wyvern_Character_BP_Fire_C",
            "Wyvern_Character_BP_Lightning_C",
            "Wyvern_Character_BP_Poison_C",
            "Ragnarok_Wyvern_Override_Ice_C",
            "Yeti_Character_BP_C"
        };

        public WildCreaturesController(ArkContextManager contextManager, IConfig config) : base(config)
        {
            _contextManager = contextManager;
        }

        public WildCreaturesViewModel Get(string id)
        {
            var context = _contextManager.GetServer(id);
            if (context == null) return null;

            var demoMode = IsDemoMode() ? new DemoMode() : null;
            var result = new WildCreaturesViewModel
            {
            };

            // access control
            var incWildCreatures = HasFeatureAccess("server", "wildcreatures", id);
            var incWildCreaturesCoords = HasFeatureAccess("server", "wildcreatures-coords", id);
            var incWildCreaturesBaseStats = HasFeatureAccess("server", "wildcreatures-basestats", id);
            var incStatistics = HasFeatureAccess("server", "wildcreatures-statistics", id);

            var speciesGroups = context.WildCreatures.GroupBy(x => x.ClassName)
                .ToDictionary(x => x.Key, x => new { items = x.ToArray(), aliases = ArkSpeciesAliases.Instance.GetAliases(x.Key) });

            if (incWildCreatures)
            {
                var species = speciesGroups.Select(x =>
                {
                    var vms = new WildCreatureSpeciesViewModel
                    {
                        ClassName = x.Key,
                        Name = x.Value.aliases?.FirstOrDefault(),
                        Aliases = x.Value.aliases?.Skip(2).ToArray() ?? new string[] { }, //skip primary name and class name
                        IsTameable = !UntameableClassNames.Contains(x.Key, StringComparer.OrdinalIgnoreCase)
                    };

                    vms.Creatures.AddRange(x.Value.items.Select(y =>
                    {
                        var vmc = new WildCreatureViewModel
                        {
                            Id1 = y.Id1,
                            Id2 = y.Id2,
                            Gender = y.Gender.ToString(),
                            BaseLevel = y.BaseLevel,
                            IsTameable = y.IsTameable
                        };

                        if (incWildCreaturesCoords)
                        {
                            vmc.X = y.Location?.X.Round(0);
                            vmc.Y = y.Location?.Y.Round(0);
                            vmc.Z = y.Location?.Z.Round(0);
                            vmc.Latitude = y.Location?.Latitude;
                            vmc.Longitude = y.Location?.Longitude;
                            vmc.TopoMapX = y.Location?.TopoMapX;
                            vmc.TopoMapY = y.Location?.TopoMapY;
                        }


                        if (incWildCreaturesBaseStats)
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

                        vmc.BaseStats = new CreatureBaseStatsViewModel
                            {
                                Health = y.BaseStats[0],
                                Stamina = y.BaseStats[1],
                                Oxygen = y.BaseStats[3],
                                Food = y.BaseStats[4],
                                Weight = y.BaseStats[7],
                                Melee = y.BaseStats[8],
                                MovementSpeed = y.BaseStats[9]
                            };
                        }

                        return vmc;
                    }).OrderByDescending(y => y.BaseLevel).ThenBy(y => y.Gender));

                    return vms;
                }).ToArray();

                foreach (var s in species)
                {
                    result.Species.Add(s.ClassName, s);
                }
            }

            var totalCount = context.WildCreatures.Length;
            var stats = new WildCreatureStatistics
            {
                CreatureCount = totalCount
            };

            if (incStatistics)
            {
                stats.Species.AddRange(speciesGroups.Select(x =>
                {
                    var vmcs = new WildCreatureSpeciesStatistics
                    {
                        ClassName = x.Key,
                        Name = x.Value.aliases?.FirstOrDefault(),
                        Aliases = x.Value.aliases?.Skip(2).ToArray() ?? new string[] { }, //skip primary name and class name
                        Count = x.Value.items.Count(),
                        Fraction = x.Value.items.Count() / (float)totalCount
                    };

                    return vmcs;
                }).OrderBy(x => x.Name).ThenByDescending(x => x.Count));
            }

            result.Statistics = stats;

            return result;
        }
    }
}
