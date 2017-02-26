using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class Config : IConfig
    {
        public Config()
        {
            ArkMultipliers = new ArkMultipliersConfigSection();
        }

        [JsonProperty(PropertyName = "botId")]
        [Description("Simple non-whitespace or special character ID to identify the bot (A-Za-z0-9)")]
        public string BotId { get; set; }

        [JsonProperty(PropertyName = "botName")]
        [Description("Short name to identify the bot")]
        public string BotName { get; set; }

        [JsonProperty(PropertyName = "botNamespace")]
        [Description("Unique namespace url given to the bot (may be same as botUrl)")]
        public string BotNamespace { get; set; }

        [JsonProperty(PropertyName = "botUrl")]
        [Description("Website url associated with the bot or ARK server (optional).")]
        public string BotUrl { get; set; }

        [JsonProperty(PropertyName = "saveFilePath")]
        [Description("Absolute file path of the .ark save file to monitor/extract data from.")]
        public string SaveFilePath { get; set; }

        [JsonProperty(PropertyName = "clusterSavePath")]
        [Description("The directory path where cluster save data is stored.")]
        public string ClusterSavePath { get; set; }

        [JsonProperty(PropertyName = "arktoolsExecutablePath")]
        [Description("File path of the ark-tools executable used to extract data from Ark (should normally be a relative path to the ark-tools.exe packaged with this application).")]
        public string ArktoolsExecutablePath { get; set; }

        [JsonProperty(PropertyName = "jsonOutputDirPath")]
        [Description("An existing directory path where temporary json files can be stored during extraction.")]
        public string JsonOutputDirPath { get; set; }

        [JsonProperty(PropertyName = "tempFileOutputDirPath")]
        [Description("An existing directory path where temporary binary files can be stored (map-images etc.)")]
        public string TempFileOutputDirPath { get; set; }

        [JsonProperty(PropertyName = "botToken")]
        [Description("Bot authentication token from https://discordapp.com/developers")]
        public string BotToken { get; set; }

        [JsonProperty(PropertyName = "steamOpenIdRelyingServiceListenPrefix")]
        [Description("Http listen prefix for Steam OpenID Relying Party webservice (requires a port that is open to external connections)")]
        public string SteamOpenIdRelyingServiceListenPrefix { get; set; }

        [JsonProperty(PropertyName = "steamOpenIdRedirectUri")]
        [Description("Publicly accessible url for incoming Steam OpenID Relying Party webservice connections (requires a port that is open to external connections)")]
        public string SteamOpenIdRedirectUri { get; set; }

        [JsonProperty(PropertyName = "googleApiKey")]
        [Description("Google API key used for url-shortening services.")]
        public string GoogleApiKey { get; set; }

        [JsonProperty(PropertyName = "steamApiKey")]
        [Description("Steam API key used for fetching user information.")]
        public string SteamApiKey { get; set; }

        [JsonProperty(PropertyName = "debugNoExtract")]
        [Description("Skips ark-tool extraction and uses already extracted files in json output directory path.")]
        public bool DebugNoExtract { get; set; }

        [JsonProperty(PropertyName = "debug")]
        [Description("Run tool with debug option set (includes experimental features etc.)")]
        public bool Debug { get; set; }

        [JsonProperty(PropertyName = "arkMultipliers")]
        [Description("Server specific multipliers.")]
        public ArkMultipliersConfigSection ArkMultipliers { get; set; }
    }

    public class ArkMultipliersConfigSection
    {
        public ArkMultipliersConfigSection()
        {
            EggHatchSpeedMultiplier = 1d;
            BabyMatureSpeedMultiplier = 1d;
            CuddleIntervalMultiplier = 1d;
        }

        [JsonProperty(PropertyName = "eggHatchSpeedMultiplier")]
        [Description("Pregnancy/incubation time multiplier.")]
        public double EggHatchSpeedMultiplier { get; set; }

        [JsonProperty(PropertyName = "babyMatureSpeedMultiplier")]
        [Description("Baby mature time multiplier.")]
        public double BabyMatureSpeedMultiplier { get; set; }

        [JsonProperty(PropertyName = "cuddleIntervalMultiplier")]
        [Description("Multiplier for duration between cuddles.")]
        public double CuddleIntervalMultiplier { get; set; }
    }
}
