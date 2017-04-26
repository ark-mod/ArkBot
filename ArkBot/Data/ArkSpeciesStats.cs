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
        private const string _speciesstatsFileName = @"arkbreedingstats-values.json";
        private object _lock = new object();
        private bool _isupdating = false;
        private Task _updateTask;

        public static ArkSpeciesStats Instance { get { return _instance ?? (_instance = new ArkSpeciesStats()); } }
        private static ArkSpeciesStats _instance;

        public ArkSpeciesStatsData Data { get; set; }

        public ArkSpeciesStats()
        {
        }

        public async Task LoadOrUpdate()
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
                            try
                            {
                                //this resource contains species stats that we need
                                await DownloadHelper.DownloadFile(
                                    @"https://raw.githubusercontent.com/cadon/ARKStatsExtractor/master/ARKBreedingStats/values.json",
                                    _speciesstatsFileName,
                                    true,
                                    TimeSpan.FromDays(1)
                                );
                            }
                            catch { /*ignore exceptions */ }


                            //even if download failed try with local file if it exists
                            if (File.Exists(_speciesstatsFileName))
                            {
                                using (var reader = new StreamReader(_speciesstatsFileName))
                                {
                                    var data = JsonConvert.DeserializeObject<ArkSpeciesStatsData>(await reader.ReadToEndAsync());
                                    if (data != null) Data = data;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logging.LogException($"Error when attempting to read {_speciesstatsFileName}", ex, typeof(ArkSpeciesStats), LogLevel.ERROR, ExceptionLevel.Ignored);
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
    }

    public class ArkSpeciesStatsData
    {
        public static ArkSpeciesStatsData Instance { get { return _instance ?? (_instance = new ArkSpeciesStatsData()); } }
        private static ArkSpeciesStatsData _instance;

        public ArkSpeciesStatsData()
        {
            StatMultipliers = new double[0][];
            SpeciesStats = new List<SpeciesStat>();
            SpeciesNames = new List<string>();
        }

        /// <summary>
        /// These are the default stat multipliers (on official servers)
        /// </summary>
        [JsonProperty("statMultipliers")]
        public double[][] StatMultipliers { get; set; }

        [JsonProperty("species")]
        public List<SpeciesStat> SpeciesStats { get; set; }

        [JsonProperty("speciesNames")]
        public List<string> SpeciesNames { get; set; }

        /// <summary>
        /// Stats in order: Health, Stamina, Oxygen, Food, Weight, Damage, Speed, Torpor
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

            [JsonProperty("statsRaw")]
            public double[][] Stats { get; set; }

            [JsonProperty("breeding")]
            public SpeciesStatBreeding Breeding { get; set; }
        }

        public class SpeciesStatBreeding : IArkMultiplierAdjustable<SpeciesStatBreeding>
        {
            public SpeciesStatBreeding()
            {

            }

            [JsonProperty("pregnancyTime")]
            public double PregnancyTime { get; set; }

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
                    PregnancyTime = PregnancyTime / config.EggHatchSpeedMultiplier,
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

        public enum Stat { Health, Stamina, Oxygen, Food, Weight, Damage, Speed, Torpor }

        public double? GetMaxValue(string[] speciesaliases, Stat stat, int baseLevel, int tamedLevel, double tamingEfficiency, double imprintingBonus = 0)
        {
            var index = (int)stat;
            var multipliers = index < StatMultipliers?.Length ? StatMultipliers.ElementAt(index) : null;
            var stats = SpeciesStats?.FirstOrDefault(x => speciesaliases.Contains(x.Name, StringComparer.OrdinalIgnoreCase))?.Stats;

            if (multipliers == null || multipliers.Length != 4 || stats == null) return null;

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
