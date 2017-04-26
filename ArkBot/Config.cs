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
            // Default values
            ArkMultipliers = new ArkMultipliersConfigSection();
            Servers = new ServerConfigSection[] { };
            Clusters = new ClusterConfigSection[] { };
            DiscordBotEnabled = true;
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

        [JsonProperty(PropertyName = "debugSaveFilePath")]
        [Description("Debug template path for saveFilePath.")]
        public string DebugSaveFilePath { get; set; }

        [JsonProperty(PropertyName = "debugClusterSavePath")]
        [Description("Debug template path for clusterSavePath.")]
        public string DebugClusterSavePath { get; set; }

        [JsonProperty(PropertyName = "debugJsonOutputDirPath")]
        [Description("Debug template path for jsonOutputDirPath.")]
        public string DebugJsonOutputDirPath { get; set; }

        [JsonProperty(PropertyName = "arkMultipliers")]
        [Description("Server specific multipliers.")]
        public ArkMultipliersConfigSection ArkMultipliers { get; set; }

        [JsonProperty(PropertyName = "disableDeveloperFetchSaveData")]
        [Description("Diable users in \"developer\"-role fetching json or save file data.")]
        public bool DisableDeveloperFetchSaveData { get; set; }

        [JsonProperty(PropertyName = "adminRoleName")]
        [Description("The name of the admin role in Discord.")]
        public string AdminRoleName { get; set; }

        [JsonProperty(PropertyName = "developerRoleName")]
        [Description("The name of the developer role in Discord.")]
        public string DeveloperRoleName { get; set; }

        [JsonProperty(PropertyName = "memberRoleName")]
        [Description("The name of the member role in Discord.")]
        public string MemberRoleName { get; set; }

        [JsonProperty(PropertyName = "serverIp")]
        [Description("The IP address used to connect to the ARK server.")]
        public string ServerIp { get; set; }

        [JsonProperty(PropertyName = "serverPort")]
        [Description("The port used to connect to the ARK server.")]
        public int ServerPort { get; set; }

        [JsonProperty(PropertyName = "rconPort")]
        [Description("The port used to connect to the ARK server over rcon.")]
        public int RconPort { get; set; }

        [JsonProperty(PropertyName = "enabledChannels")]
        [Description("A list of channels where the bot will listen to and answer commands.")]
        public string[] EnabledChannels { get; set; }

        [JsonProperty(PropertyName = "infoTopicChannel")]
        [Description("Channel where topic is set to display information about last update, next update and how to use bot commands.")]
        public string InfoTopicChannel { get; set; }

        [JsonProperty(PropertyName = "announcementChannel")]
        [Description("Channel where announcements are made (votes etc.)")]
        public string AnnouncementChannel { get; set; }

        [JsonProperty(PropertyName = "rconPassword")]
        [Description("The password used to connect to the server via rcon.")]
        public string RconPassword { get; set; }

        [JsonProperty(PropertyName = "updateServerBatchFilePath")]
        [Description("Absolute file path of a batch file to run in order to update the server.")]
        public string UpdateServerBatchFilePath { get; set; }

        [JsonProperty(PropertyName = "startServerBatchFilePath")]
        [Description("Absolute file path of a batch file to run in order to start the server.")]
        public string StartServerBatchFilePath { get; set; }

        [JsonProperty(PropertyName = "backupsEnabled")]
        [Description("Option to enable savegame backups.")]
        public bool BackupsEnabled { get; set; }

        [JsonProperty(PropertyName = "backupsDirectoryPath")]
        [Description("Directory path where savegame backups are stored.")]
        public string BackupsDirectoryPath { get; set; }

        [JsonProperty(PropertyName = "discordBotEnabled")]
        [Description("Option to enable/disable the discord bot component.")]
        public bool DiscordBotEnabled { get; set; }

        [JsonProperty(PropertyName = "webApiListenPrefix")]
        [Description("Http listen prefix for WebAPI service (requires a port that is open to external connections.")]
        public string WebApiListenPrefix { get; set; }

        [JsonProperty(PropertyName = "servers")]
        [Description("Server instance configurations.")]
        public ServerConfigSection[] Servers { get; set; }

        [JsonProperty(PropertyName = "clusters")]
        [Description("Cluster instance configurations.")]
        public ClusterConfigSection[] Clusters { get; set; }
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

    public class ServerConfigSection
    {
        public ServerConfigSection()
        {
        }

        [JsonProperty(PropertyName = "key")]
        [Description("Unique key/name for this server instance.")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "cluster")]
        [Description("Optional key for the cluster instance this server is part of.")]
        public string Cluster { get; set; }

        [JsonProperty(PropertyName = "saveFilePath")]
        [Description("Absolute file path of the .ark save file to monitor/extract data from.")]
        public string SaveFilePath { get; set; }

        [JsonProperty(PropertyName = "ip")]
        [Description("The IP address used to connect to this server instance.")]
        public string Ip { get; set; }

        [JsonProperty(PropertyName = "port")]
        [Description("The port used to connect to this server instance.")]
        public int Port { get; set; }

        [JsonProperty(PropertyName = "rconPort")]
        [Description("The port used to connect to this server instance over rcon.")]
        public int RconPort { get; set; }
    }

    public class ClusterConfigSection
    {
        public ClusterConfigSection()
        {
        }

        [JsonProperty(PropertyName = "key")]
        [Description("Unique key/name for this cluster instance.")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "savePath")]
        [Description("The directory path where cluster save data is stored.")]
        public string SavePath { get; set; }
    }
}
