using ArkBot.Configuration.Model;
using ArkBot.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Data
{
   

    public class ArkSpeciesStats
    {
        private const string _obeliskUrl = @"https://raw.githubusercontent.com/arkutils/Obelisk/master/data/asb/";

        private const string _valuesUrl = @"values.json";
        private const string _manifestUrl = @"_manifest.json";

        private const string _valuesFileName = @"obelisk-abs-species.json";
        private const string _manifestFileName = @"obelisk-asb-manifest.json";

        private object _lock = new object();
        private Task _updateTask;

        public static ArkSpeciesStats Instance { get { return _instance ?? (_instance = new ArkSpeciesStats()); } }
        private static ArkSpeciesStats _instance;

        public ArkSpeciesStatsData Data { get; set; }

        public ObeliskManifest Manifest { get; set; }

        private ArkSpeciesStatsData Values { get; set; }

        private List<ArkSpeciesStatsData> Mods { get; set; } = new List<ArkSpeciesStatsData>();

        public ArkSpeciesStats()
        {
        }

        public async Task LoadOrUpdate(int[] modIds)
        {
            Task updateTask = null;
            lock (_lock)
            {
                if (_updateTask == null)
                {
                    updateTask = _updateTask = Task.Run(async () =>
                    {
                        try
                        {
                            // values.json
                            var data = await DownloadResource<ArkSpeciesStatsData>(_obeliskUrl + _valuesUrl, _valuesFileName);
                            if (data != null) Values = data;

                            // _manifest.json
                            var manifest = await DownloadResource<ObeliskManifest>(_obeliskUrl + _manifestUrl, _manifestFileName);
                            if (manifest != null) Manifest = manifest;

                            // mods
                            if (modIds?.Length > 0)
                            {
                                foreach (var modId in modIds)
                                {
                                    var strModId = modId.ToString();
                                    var mod = Manifest?.Files?.FirstOrDefault(x => x.Value?.Mod?.Id.Equals(strModId) == true);

                                    var modData = await DownloadResource<ArkSpeciesStatsData>(mod.HasValue ? _obeliskUrl + mod.Value.Key : null, $"obelisk-asb-species-{modId}.json", skipDownload: mod == null);
                                    if (modData != null)
                                    {
                                        ViewModel.Workspace.Instance.Console.AddLog("Loaded species data for " + (mod.HasValue ? $"{mod.Value.Value.Mod.Title} ({modId})" : $"'{modId}'") + ".");
                                        Mods.Add(modData);
                                    }
                                    else
                                    {
                                        ViewModel.Workspace.Instance.Console.AddLog($"Mod '{modId}' is not supported and could result in some data missing from the web app.");
                                    }
                                }
                            }

                            Data = new ArkSpeciesStatsData(Values, Mods);
                        }
                        finally
                        {
                            lock(_lock)
                            {
                                _updateTask = null;
                            }
                        }
                    });
                } else updateTask = _updateTask;
            }

            await updateTask;
        }

        private async Task<TValue> DownloadResource<TValue>(string url, string path, bool skipDownload = false) where TValue: class
        {
            try
            {
                if (!skipDownload)
                {
                    try
                    {
                        //this resource contains species stats that we need
                        await DownloadHelper.DownloadFile(
                            url,
                            path,
                            true,
                            TimeSpan.FromDays(1)
                        );
                    }
                    catch (Exception ex)
                    {
                        /*ignore exceptions */
                        Logging.LogException($"Error downloading {url}", ex, typeof(ArkSpeciesStats), LogLevel.WARN, ExceptionLevel.Ignored);
                    }
                }

                //even if download failed try with local file if it exists
                if (File.Exists(path))
                {
                    using (var reader = new StreamReader(path))
                    {
                        var json = await reader.ReadToEndAsync();
                        var data = JsonConvert.DeserializeObject<TValue>(json);
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException($"Error when attempting to read {path}", ex, typeof(ArkSpeciesStats), LogLevel.ERROR, ExceptionLevel.Ignored);
            }

            return null;
        }
    }

    public class ObeliskManifest
    {
        public string Format { get; set; }
        public Dictionary<string, ObeliskFile> Files { get; set; } = new Dictionary<string, ObeliskFile>();
    }

    public class ObeliskFile
    {
        public string Version { get; set; }
        public ObeliskMod Mod { get; set; }
    }

    public class ObeliskMod
    {
        public string Id { get; set; }
        public string Tag { get; set; }
        public string Title { get; set; }
    }

    public class ArkSpeciesStatsData
    {
        public ArkSpeciesStatsData()
        {
            SpeciesStats = new List<SpeciesStat>();
        }

        public ArkSpeciesStatsData(ArkSpeciesStatsData values, List<ArkSpeciesStatsData> mods) : this()
        {
            if (values?.SpeciesStats != null) SpeciesStats.AddRange(values.SpeciesStats);

            foreach (var mod in mods)
            {
                if (mod?.SpeciesStats != null) SpeciesStats.AddRange(mod.SpeciesStats);
            }
        }

        [JsonProperty("species")]
        public List<SpeciesStat> SpeciesStats { get; set; }

        /// <summary>
        /// Stats in order: Health, Stamina, Torpidity, Oxygen, Food, Water, Temperature, Weight, MeleeDamage, MovementSpeed, Fortitude, CraftingSpeed
        /// </summary>
        public class SpeciesStat
        {
            public SpeciesStat()
            {
                Stats = new double[0][];
                Breeding = new SpeciesStatBreeding();
            }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("blueprintPath")]
            public string BlueprintPath { get; set; }

            [JsonProperty("fullStatsRaw")]
            public double[][] Stats { get; set; }

            [JsonProperty("breeding")]
            public SpeciesStatBreeding Breeding { get; set; }
        }

        public class SpeciesStatBreeding : IArkMultiplierAdjustable<SpeciesStatBreeding>
        {
            public SpeciesStatBreeding()
            {

            }

            [JsonProperty("gestationTime")]
            public double GestationTime { get; set; }

            [JsonProperty("incubationTime")]
            public double IncubationTime { get; set; }

            [JsonProperty("maturationTime")]
            public double MaturationTime { get; set; }

            [JsonProperty("matingCooldownMin")]
            public double MatingCooldownMin { get; set; }

            [JsonProperty("matingCooldownMax")]
            public double MatingCooldownMax { get; set; }

            [JsonProperty("eggTempMin")]
            public double EggTempMin { get; set; }

            [JsonProperty("eggTempMax")]
            public double EggTempMax { get; set; }

            [JsonIgnore]
            internal protected bool _isAdjusted = false;
            public SpeciesStatBreeding GetAdjusted(ArkMultipliersConfigSection config)
            {
                return _isAdjusted ? this : new SpeciesStatBreeding
                {
                    GestationTime = GestationTime / config.EggHatchSpeedMultiplier,
                    IncubationTime = IncubationTime / config.EggHatchSpeedMultiplier,
                    MaturationTime = MaturationTime / config.BabyMatureSpeedMultiplier,
                    MatingCooldownMin = MatingCooldownMin,
                    MatingCooldownMax = MatingCooldownMax,
                    EggTempMin = EggTempMin,
                    EggTempMax = EggTempMax,
                    _isAdjusted = true
                };
            }
        }

        public interface IArkMultiplierAdjustable<TClass>
        {
            TClass GetAdjusted(ArkMultipliersConfigSection config);
        }

        public enum Stat { Health, Stamina, Torpidity, Oxygen, Food, Water, Temperature, Weight, MeleeDamage, MovementSpeed, Fortitude, CraftingSpeed }

        public SpeciesStat GetSpecies(string[] speciesaliases)
        {
            var byName = SpeciesStats?.FirstOrDefault(x => speciesaliases.Contains(x.Name, StringComparer.OrdinalIgnoreCase));
            if (byName != null) return byName;

            var className = speciesaliases.FirstOrDefault(x => x.EndsWith("_C"));
            if (className != null)
            {
                className = "." + className.Substring(0, className.Length - 2);
                var byClass = SpeciesStats?.Where(x => x.BlueprintPath.EndsWith(className, StringComparison.OrdinalIgnoreCase)).Take(2).ToArray();
                return byClass.Length == 1 ? byClass[0] : null;
            }

            return null;
        }

        public double? GetMaxValue(string[] speciesaliases, Stat stat, int baseLevel, int tamedLevel, double tamingEfficiency, double imprintingBonus = 0)
        {
            var index = (int)stat;
            var multipliers = ArkServerMultipliers.Instance.Data?.GetStatMultipliers(stat);
            var stats = GetSpecies(speciesaliases)?.Stats;

            if (multipliers == null || multipliers.Length != 4 || stats == null || stats[index] == null) return null;

            //stats = new double[8].Select((x, i) =>
            //{
            //    var v = new double[5];
            //    stats[i]?.CopyTo(v, 0); //copy raw
            //    v[1] *= multipliers[3]; //inc. per wild level 
            //    v[2] *= multipliers[2]; //inc. per tamed level
            //    if(v[3] > 0) v[3] *= multipliers[0]; //add. when tamed
            //    v[4] *= multipliers[1]; //multi. affinity
            //    return v;
            //}).ToArray();

            //stats: Base value, Increase per wild level, increase per tamed level, add. when tamed, multi. affinity

            var B = stats[index][0]; //base-value
            var Lw = (double)baseLevel; //level-wild
            var Iw = stats[index][1]; //increase per wild-level as % of B
            var IwM = multipliers[3]; //increase per domesticated level modifier
            var IB = imprintingBonus;
            var IBM = 1d; //imprinting bonus multiplier
            var Ta = stats[index][3]; //additive taming-bonus
            var TaM = Ta > 0 ? multipliers[0] : 1d; //additive taming-bonus modifier (not when negative)
            var TE = tamingEfficiency; //taming efficiency
            var Tm = stats[index][4]; //multiplicative taming-bonus
            var TmM = multipliers[1]; //multiplicative taming-bonus modifier
            var Ld = (double)tamedLevel; //level points spend after taming
            var Id = stats[index][2]; //increase per domesticated level as % of B
            var IdM = multipliers[2]; //increase per domesticated level modifier


            //Each dino has 6 variables that affect the final stat:

            //Base-value: B
            //increase per wild-level as % of B: Iw
            //increase per domesticated level as % of B: Id
            //additive taming-bonus: Ta (not when negative)
            //multiplicative taming-bonus: Tm
            //imprinting bonus: IB
            //The imprinting bonus IB only affects bred creatures, and all stat-values except of stamina and oxygen (it does affect the torpor-value).

            //The game has further global variables for each stat, that affect the variables above:

            //additive taming-bonus modifier: TaM
            //multiplicative taming-bonus modifier: TmM
            //increase per domesticated level modifier: IdM
            //The imprinting bonus is scaled by the global variable (default: 1)

            //BabyImprintingStatScaleMultiplier: IBM
            //Currently these modifiers are for health TaM = 0.14, TmM = 0.44 and IdM = 0.2, and for melee damage TaM = 0.14, TmM = 0.44 and IdM = 0.17.

            //Assume that a creature has upleveled a certain stat in the wild Lw times, was upleveled a stat by a player Ld times and was tamed with a taming effectiveness of TE. Then the final value V of that stat (as you see it ingame) is

            //Ta and Tm are 0 in the most cases. For Health Ta is always 0.5 (except for the Giganotosaurus, where it is -110000), for Damage Ta is in most cases 0.5 and Tm = 0.4 (that gives the creature a +50pp (percentage point) bonus and after this the value is multiplied by (1 + 0.4 * TmM * TE (so it get's another 40% * 0.45 = 18% of the current value, dependent on the taming effectiveness).

            //Vw = B × ( 1 + Lw × Iw) //wild value
            //Vpt = (Vw + Ta × TaM) × (1 + TE × Tm × TmM) //post-tamed
            //Vpt = (Vw × (1 + IB × 0.2 × IBM) + Ta × TaM) × (1 + TE × Tm × TmM) //bred creatures
            //V = Vpt × (1 + Ld × Id × IdM) //final value
            //V = (B × ( 1 + Lw × Iw) × (1 + IB × 0.2 × IBM) + Ta × TaM) × (1 + TE × Tm × TmM) × (1 + Ld × Id × IdM) //final value (full equation)

            var Vw = B * (1 + Lw * Iw * IwM); //wild value
            var Vib = stat == Stat.Stamina || stat == Stat.Oxygen ? 1 : (1 + IB * 0.2 * IBM); //imprinting bonus
            var Vpt = (Vw * Vib + Ta * TaM) * (1 + TE * Tm * TmM); //post-tame and bred creatures
            var V = Vpt * (1 + Ld * Id * IdM); //final value

            return V;
        }
    }
}
