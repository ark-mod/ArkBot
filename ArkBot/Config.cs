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
            Ssl = new SslConfigSection();
            UserRoles = new UserRolesConfigSection();
            ArkMultipliers = new ArkMultipliersConfigSection();
            Servers = new ServerConfigSection[] { };
            Clusters = new ClusterConfigSection[] { };
            DiscordBotEnabled = true;
            WebAppRedirectListenPrefix = new string[] { };
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

        [JsonProperty(PropertyName = "appUrl")]
        [Description("External url pointing to the Web App (optional).")]
        public string AppUrl { get; set; }

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

        [JsonProperty(PropertyName = "userRoles")]
        [Description("Role based access control.")]
        public UserRolesConfigSection UserRoles { get; set; }

        [JsonProperty(PropertyName = "enabledChannels")]
        [Description("A list of channels where the bot will listen to and answer commands.")]
        public string[] EnabledChannels { get; set; }

        [JsonProperty(PropertyName = "infoTopicChannel")]
        [Description("Channel where topic is set to display information about last update, next update and how to use bot commands.")]
        public string InfoTopicChannel { get; set; }

        [JsonProperty(PropertyName = "announcementChannel")]
        [Description("Channel where announcements are made (votes etc.)")]
        public string AnnouncementChannel { get; set; }

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
        [Description("Http listen prefix for WebAPI service (requires a port that is open to external connections) [The prebuilt web-app included in this release is by default configured to call the web api on 127.0.0.1:60001. If you want to use another port for the web api you will need to reflect this change in environment.prod.ts and rebuild the web-app dist manually.]")]
        public string WebApiListenPrefix { get; set; }

        [JsonProperty(PropertyName = "webAppListenPrefix")]
        [Description("Http listen prefix for Web App (requires a port that is open to external connections)")]
        public string WebAppListenPrefix { get; set; }

        [JsonProperty(PropertyName = "webAppRedirectListenPrefix")]
        [Description("Http listen prefix(es) that are redirected to BotUrl.")]
        public string[] WebAppRedirectListenPrefix { get; set; }

        [JsonProperty(PropertyName = "powershellFilePath")]
        [Description("Absolute file path of the powershell executable (only used with Server.UsePowershellOutputRedirect)")]
        public string PowershellFilePath { get; set; }

        [JsonProperty(PropertyName = "useCompatibilityChangeWatcher")]
        [Description("Use timer based .ark save file watcher rather than the default (based on FileSystemWatcher)")]
        public bool UseCompatibilityChangeWatcher { get; set; }

        [JsonProperty(PropertyName = "ssl")]
        [Description("Configure Web App and WebAPI to use SSL with a free certificate from Lets Encrypt")]
        public SslConfigSection Ssl { get; set; }

        [JsonProperty(PropertyName = "servers")]
        [Description("Server instance configurations.")]
        public ServerConfigSection[] Servers { get; set; }

        [JsonProperty(PropertyName = "clusters")]
        [Description("Cluster instance configurations.")]
        public ClusterConfigSection[] Clusters { get; set; }
    }

    public class UserRolesConfigSection
    {
        public UserRolesConfigSection()
        {
            Admins = new string[] { };
        }

        [JsonProperty(PropertyName = "admins")]
        [Description("Collection of steam id(s) belonging to the admin role.")]
        public string[] Admins { get; set; }
    }

    public class SslConfigSection
    {
        public SslConfigSection()
        {
            Domains = new string[] { };
        }

        [JsonProperty(PropertyName = "enabled")]
        [Description("Toggle ssl.")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "challengeListenPrefix")]
        [Description("Http listen prefix for ssl challenge request (external port must be 80)")]
        public string ChallengeListenPrefix { get; set; }

        [JsonProperty(PropertyName = "name")]
        [Description("Friendly name of the certificate.")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "password")]
        [Description("Private password.")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "email")]
        [Description("Registration contact email.")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "domains")]
        [Description("Domain name(s) to issue the certificate for.")]
        public string[] Domains { get; set; }

        [JsonProperty(PropertyName = "ports")]
        [Description("Ports to bind the ssl certificate to.")]
        public int[] Ports { get; set; }

        [JsonProperty(PropertyName = "useCompatibilityNonSNIBindings")]
        [Description("Use non SNI SSL bindings for previous Windows OS (before Windows 8/2012)")]
        public bool UseCompatibilityNonSNIBindings { get; set; }
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

        [JsonProperty(PropertyName = "rconPassword")]
        [Description("The password used to connect to this server instance via rcon.")]
        public string RconPassword { get; set; }

        //[JsonProperty(PropertyName = "updateBatchFilePath")]
        //[Description("Absolute file path of a batch file to run in order to update the server.")]
        //public string UpdateBatchFilePath { get; set; }

        //[JsonProperty(PropertyName = "startBatchFilePath")]
        //[Description("Absolute file path of a batch file to run in order to start the server.")]
        //public string StartBatchFilePath { get; set; }

        [JsonProperty(PropertyName = "serverExecutablePath")]
        [Description("Absolute file path of the server instance executable.")]
        public string ServerExecutablePath { get; set; }

        [JsonProperty(PropertyName = "serverExecutableArguments")]
        [Description("Command line arguments used when starting the server instance.")]
        public string ServerExecutableArguments { get; set; }

        [JsonProperty(PropertyName = "steamCmdExecutablePath")]
        [Description("Absolute file path of the steamcmd executable.")]
        public string SteamCmdExecutablePath { get; set; }

        [JsonProperty(PropertyName = "serverInstallDirPath")]
        [Description("The directory path to force steamcmd updates to.")]
        public string ServerInstallDirPath { get; set; }

        [JsonProperty(PropertyName = "usePowershellOutputRedirect")]
        [Description("Use alternative powershell/file based output redirect for update progress notifications.")]
        public bool UsePowershellOutputRedirect { get; set; }

        [JsonProperty(PropertyName = "disableChatNotificationOnGlobalCountdown")]
        [Description("Disable chat notifications for this server instance when trigged by admin multiple server countdown (feature is used for compatibility with cross server chat).")]
        public bool DisableChatNotificationOnGlobalCountdown { get; set; }
        
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
