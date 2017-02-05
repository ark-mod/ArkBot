using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot
{
    public class Config
    {
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

        [JsonProperty(PropertyName = "debugNoExtract")]
        [Description("Skips ark-tool extraction and uses already extracted files in json output directory path.")]
        public bool DebugNoExtract { get; set; }
    }
}
