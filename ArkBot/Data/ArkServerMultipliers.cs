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
   

    public class ArkServerMultipliers
    {
        private const string _serverMultipliersUrl = @"https://raw.githubusercontent.com/cadon/ARKStatsExtractor/master/ARKBreedingStats/json/serverMultipliers.json";
        private const string _serverMultipliersFileName = @"arkbreedingstats-serverMultipliers.json";
        private object _lock = new object();
        private Task _updateTask;

        public static ArkServerMultipliers Instance { get { return _instance ?? (_instance = new ArkServerMultipliers()); } }
        private static ArkServerMultipliers _instance;

        public ArkServerMultipliersData Data { get; set; }

        public ArkServerMultipliers()
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
                                //this resource contains default values for server multipliers that we need
                                await DownloadHelper.DownloadFile(
                                    _serverMultipliersUrl,
                                    _serverMultipliersFileName,
                                    true,
                                    TimeSpan.FromDays(1)
                                );
                            }
                            catch (Exception ex)
                            {
                                /*ignore exceptions */
                                Logging.LogException($"Error downloading {_serverMultipliersUrl}", ex, typeof(ArkServerMultipliers), LogLevel.WARN, ExceptionLevel.Ignored);
                            }

                            //even if download failed try with local file if it exists
                            if (File.Exists(_serverMultipliersFileName))
                            {
                                using (var reader = new StreamReader(_serverMultipliersFileName))
                                {
                                    var json = await reader.ReadToEndAsync();
                                    var data = JsonConvert.DeserializeObject<ArkServerMultipliersData>(json);
                                    if (data != null) Data = data;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logging.LogException($"Error when attempting to read {_serverMultipliersFileName}", ex, typeof(ArkServerMultipliers), LogLevel.ERROR, ExceptionLevel.Ignored);
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

    public class ArkServerMultipliersData
    {
        private readonly double[] _default = new [] { 1d, 1d, 1d, 1d };

        public ArkServerMultipliersData()
        {
            ServerMultipliers = new Dictionary<string, ArkServerMultipliersEntity>();
        }

        /// <summary>
        /// Default stat multipliers for keyed server types (official, singleplayer, Small Tribes, ARKpocalypse, Classic PvP)
        /// </summary>
        [JsonProperty("serverMultiplierDictionary")]
        public Dictionary<string, ArkServerMultipliersEntity> ServerMultipliers { get; set; }

        public double[] GetStatMultipliers(ArkSpeciesStatsData.Stat stat, string key = "official")
        {
            ServerMultipliers.TryGetValue(key, out var value);

            var index = (int)stat;
            var multipliers = index < value?.StatMultipliers?.Length ? value.StatMultipliers.ElementAt(index) ?? _default : null;

            return multipliers;
        }
    }

    public class ArkServerMultipliersEntity
    {
        public ArkServerMultipliersEntity()
        {
            StatMultipliers = new double[0][];
        }

        /// <summary>
        /// Default stat multipliers or null
        /// </summary>
        [JsonProperty("statMultipliers")]
        public double[][] StatMultipliers { get; set; }

        public double? TamingSpeedMultiplier { get; set; }
        public double? MatingIntervalMultiplier { get; set; }
        public double? EggHatchSpeedMultiplier { get; set; }
        public double? BabyMatureSpeedMultiplier { get; set; }
        public double? BabyCuddleIntervalMultiplier { get; set; }
    }
}
