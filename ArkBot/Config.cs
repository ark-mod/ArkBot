using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Configuration;
using ArkBot.Configuration.Validation;
using Discord;
using Microsoft.IdentityModel;
using PropertyChanged;
using Validar;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace ArkBot
{
    internal static class ConfigurationCategory
    {
        /// <summary>
        /// Settings that must be changed (environment specific)
        /// </summary>
        internal const string Required = "Required";
        /// <summary>
        /// Settings that are either optional or may be left at default
        /// </summary>
        internal const string Optional = "Optional";

        /// <summary>
        /// Optional settings for advanced configurations
        /// </summary>
        internal const string Advanced = "Advanced";

        /// <summary>
        /// Optional setting for debugging, logging etc.
        /// </summary>
        internal const string Debug = "Debug";
    }

    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    [CategoryOrder(ConfigurationCategory.Required, 0)]
    [CategoryOrder(ConfigurationCategory.Optional, 1)]
    [CategoryOrder(ConfigurationCategory.Advanced, 2)]
    [CategoryOrder(ConfigurationCategory.Debug, 3)]
    public class Config : IConfig
    {
        public Config()
        {
            // Default values
            Ssl = new SslConfigSection();
            UserRoles = new Dictionary<string, string[]>();
            ArkMultipliers = new ArkMultipliersConfigSection();
            Servers = new ServerConfigSection[] { };
            Clusters = new ClusterConfigSection[] { };
            WebAppRedirectListenPrefix = new string[] { };
            AccessControl = new AccessControlConfigSection();
            Discord = new DiscordConfigSection();
            Backups = new BackupsConfigSection();
        }

        // Required

        [JsonProperty(PropertyName = "googleApiKey")]
        [DisplayName("Google API Key")]
        [Description("Google API Key used for url-shortening services")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(0)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        //todo: validate google api key with google
        public string GoogleApiKey { get; set; }

        [JsonProperty(PropertyName = "steamApiKey")]
        [DisplayName("Steam API Key")]
        [Description("Steam API Key used for fetching user information")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(1)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        //todo: validate steam api key with steam
        public string SteamApiKey { get; set; }

        [JsonProperty(PropertyName = "tempFileOutputDirPath")]
        [DisplayName("Temporary Files Directory")]
        [Description("An existing directory path where temporary binary files can be stored (zip-files etc.)")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(2)]
        [Editor(typeof(DirectoryPathEditor), typeof(DirectoryPathEditor))]
        [DirectoryExists(ErrorMessage = "{0} is not set or the directory path does not exist")]
        public string TempFileOutputDirPath { get; set; }

        [JsonProperty(PropertyName = "servers")]
        [DisplayName("Servers")]
        [Description("Server instance configurations")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(3)]
        [Required(ErrorMessage = "{0} is not set")]
        public ServerConfigSection[] Servers { get; set; }

        [JsonProperty(PropertyName = "clusters")]
        [DisplayName("Clusters")]
        [Description("Cluster instance configurations")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(4)]
        [Required(ErrorMessage = "{0} is not set")]
        public ClusterConfigSection[] Clusters { get; set; }


        // Optional

        [JsonProperty(PropertyName = "botName")]
        [DisplayName("Bot Name")]
        [Description("Short name to identify the bot")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(0)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        public string BotName { get; set; }

        [JsonProperty(PropertyName = "botUrl")]
        [DisplayName("Bot URL")]
        [Description("Website url associated with the bot or ARK server (optional)")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(1)]
        [ValidUrl(Optional = true, ErrorMessage = "{0} is not a valid URL")]
        public string BotUrl { get; set; }

        [JsonProperty(PropertyName = "appUrl")]
        [DisplayName("Web App URL")]
        [Description("External url pointing to the Web App (optional)")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(2)]
        [ValidUrl(Optional = true, ErrorMessage = "{0} is not a valid URL")]
        public string AppUrl { get; set; }

        [JsonProperty(PropertyName = "arkMultipliers")]
        [DisplayName("ARK Multipliers")]
        [Description("Server specific multipliers")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(3)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        public ArkMultipliersConfigSection ArkMultipliers { get; set; }

        [JsonProperty(PropertyName = "discord")]
        [DisplayName("Discord")]
        [Description("Discord bot settings")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(4)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        public DiscordConfigSection Discord { get; set; }

        [JsonProperty(PropertyName = "userRoles")]
        [DisplayName("User Roles")]
        [Description("Explicit steam user role assignment")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(5)]
        [Required(ErrorMessage = "{0} is not set")]
        public Dictionary<string, string[]> UserRoles { get; set; }

        [JsonProperty(PropertyName = "accessControl")]
        [DisplayName("Access Control")]
        [Description("Per-feature role based access control configuration")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(6)]
        [Required(ErrorMessage = "{0} is not set")]
        public AccessControlConfigSection AccessControl { get; set; }

        [JsonProperty(PropertyName = "backups")]
        [DisplayName("Backups")]
        [Description("Savegame backups")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(7)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        public BackupsConfigSection Backups { get; set; }

        [JsonProperty(PropertyName = "webAppRedirectListenPrefix")]
        [DisplayName("Web App Redirect Listen Prefix(es)")]
        [Description("Http listen prefix(es) that are redirected to BotUrl")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(8)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        //todo: validate this listen prefix
        public string[] WebAppRedirectListenPrefix { get; set; }

        [JsonProperty(PropertyName = "powershellFilePath")]
        [DisplayName("Powershell Executable Path")]
        [Description("Absolute file path of the powershell executable (only used with Server.UsePowershellOutputRedirect)")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(9)]
        [Editor(typeof(OpenFilePathEditor), typeof(OpenFilePathEditor))]
        [OpenFilePathEditor(Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*")]
        [FileExists(ErrorMessage = "{0} is not set or the file path does not exist")]
        public string PowershellFilePath { get; set; }

        [JsonProperty(PropertyName = "ssl")]
        [DisplayName("SSL")]
        [Description("Configure Web App and WebAPI to use SSL with a free certificate from Lets Encrypt")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(10)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        public SslConfigSection Ssl { get; set; }

        [JsonProperty(PropertyName = "webApiListenPrefix")]
        [DisplayName("Web API Listen Prefix")]
        [Description("Http listen prefix for WebAPI service (requires a port that is open to external connections) [The prebuilt web-app included in this release is by default configured to call the web api on 127.0.0.1:60001. If you want to use another port for the web api you will need to reflect this change in environment.prod.ts and rebuild the web-app dist manually.]")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(11)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        //todo: validate this listen prefix
        public string WebApiListenPrefix { get; set; }

        [JsonProperty(PropertyName = "webAppListenPrefix")]
        [DisplayName("Web App Listen Prefix")]
        [Description("Http listen prefix for Web App (requires a port that is open to external connections)")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(12)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        //todo: validate this listen prefix
        public string WebAppListenPrefix { get; set; }


        // Optional Advanced

        [JsonProperty(PropertyName = "savegameExtractionMaxDegreeOfParallelism")]
        [DisplayName("Savegame Extraction Max Degree Of Parallelism")]
        [Description("Max degree of parallelism to use for savegame extraction. Change only if experiencing out of memory exceptions")]
        [Category(ConfigurationCategory.Advanced)]
        [PropertyOrder(0)]
        [RangeOptional(1, 32, Optional = true)]
        public int? SavegameExtractionMaxDegreeOfParallelism { get; set; }

        [JsonProperty(PropertyName = "useCompatibilityChangeWatcher")]
        [DisplayName("Use Compatibility Change Watcher")]
        [Description("Use timer based .ark save file watcher rather than the default (based on FileSystemWatcher)")]
        [Category(ConfigurationCategory.Advanced)]
        [PropertyOrder(1)]
        public bool UseCompatibilityChangeWatcher { get; set; }


        // Debugging, Logging etc.

        [JsonProperty(PropertyName = "discordLogLevel")]
        [DisplayName("Discord Log Level")]
        [Description("Log level for Discord.NET")]
        [Category(ConfigurationCategory.Debug)]
        [PropertyOrder(0)]
        public LogSeverity DiscordLogLevel { get; set; }

        [JsonProperty(PropertyName = "anonymizeWebApiData")]
        [DisplayName("Anonymize Web API Data")]
        [Description("Anonymize all data in the WebAPI. Used to create data dumps for demoing the web-app")]
        [Category(ConfigurationCategory.Debug)]
        [PropertyOrder(1)]
        public bool AnonymizeWebApiData { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class DiscordConfigSection
    {
        public DiscordConfigSection()
        {
            DiscordBotEnabled = true;
            AccessControl = new AccessControlConfigSection();
        }

        [JsonProperty(PropertyName = "discordBotEnabled")]
        [DisplayName("Discord Bot Enabled")]
        [Description("Option to enable/disable the discord bot component")]
        public bool DiscordBotEnabled { get; set; }

        [JsonProperty(PropertyName = "botToken")]
        [DisplayName("Bot Token")]
        [Description("Bot authentication token from https://discordapp.com/developers")]
        public string BotToken { get; set; }

        [JsonProperty(PropertyName = "enabledChannels")]
        [DisplayName("Enabled Channels")]
        [Description("A list of channels where the bot will listen to and answer commands")]
        public string[] EnabledChannels { get; set; }

        [JsonProperty(PropertyName = "infoTopicChannel")]
        [DisplayName("Info Topic Channel")]
        [Description("Channel where topic is set to display information about last update, next update and how to use bot commands")]
        public string InfoTopicChannel { get; set; }

        [JsonProperty(PropertyName = "announcementChannel")]
        [DisplayName("Announcement Channel")]
        [Description("Channel where announcements are made (votes etc.)")]
        public string AnnouncementChannel { get; set; }

        [JsonProperty(PropertyName = "memberRoleName")]
        [DisplayName("Member Role Name")]
        [Description("The name of the member role in Discord")]
        public string MemberRoleName { get; set; }

        [JsonProperty(PropertyName = "disableDeveloperFetchSaveData")]
        [DisplayName("Disable Developer Fetch Save Data")]
        [Description("Diable users in \"developer\"-role fetching json or save file data")]
        public bool DisableDeveloperFetchSaveData { get; set; }

        [JsonProperty(PropertyName = "accessControl")]
        [DisplayName("Access Control")]
        [Description("Per-feature role based access control configuration")]
        public AccessControlConfigSection AccessControl { get; set; }

        [JsonProperty(PropertyName = "steamOpenIdRelyingServiceListenPrefix")]
        [DisplayName("Steam Open ID Relying Server Listen Prefix")]
        [Description("Http listen prefix for Steam OpenID Relying Party webservice (requires a port that is open to external connections)")]
        public string SteamOpenIdRelyingServiceListenPrefix { get; set; }

        [JsonProperty(PropertyName = "steamOpenIdRedirectUri")]
        [DisplayName("Steam Open ID Redirect URL")]
        [Description("Publicly accessible url for incoming Steam OpenID Relying Party webservice connections (requires a port that is open to external connections)")]
        public string SteamOpenIdRedirectUri { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class BackupsConfigSection
    {
        [JsonProperty(PropertyName = "backupsEnabled")]
        [DisplayName("Backups Enabled")]
        [Description("Option to enable savegame backups")]
        [PropertyOrder(0)]
        public bool BackupsEnabled { get; set; }

        [JsonProperty(PropertyName = "backupsDirectoryPath")]
        [DisplayName("Backups Directory Path")]
        [Description("Directory path where savegame backups are stored")]
        [PropertyOrder(1)]
        [DirectoryExists(IfMethod = nameof(IsBackupsEnabled), ErrorMessage = "{0} directory path does not exist")]
        public string BackupsDirectoryPath { get; set; }

        // Validation methods

        private bool IsBackupsEnabled()
        {
            return BackupsEnabled;
        }
    }

    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class SslConfigSection
    {
        public SslConfigSection()
        {
            Domains = new string[] { };
        }

        [JsonProperty(PropertyName = "enabled")]
        [DisplayName("Enabled")]
        [Description("Toggle ssl.")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "challengeListenPrefix")]
        [DisplayName("Challenge Listen Prefix")]
        [Description("Http listen prefix for ssl challenge request (external port must be 80)")]
        public string ChallengeListenPrefix { get; set; }

        [JsonProperty(PropertyName = "name")]
        [DisplayName("Name")]
        [Description("Friendly name of the certificate")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "password")]
        [DisplayName("Password")]
        [Description("Private password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "email")]
        [DisplayName("Email")]
        [Description("Registration contact email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "domains")]
        [DisplayName("Domain name(s)")]
        [Description("Domain name(s) to issue the certificate for")]
        public string[] Domains { get; set; }

        [JsonProperty(PropertyName = "ports")]
        [DisplayName("Ports")]
        [Description("Ports to bind the ssl certificate to")]
        public int[] Ports { get; set; }

        [JsonProperty(PropertyName = "useCompatibilityNonSNIBindings")]
        [DisplayName("Use Compatibility non-SNI Bindings")]
        [Description("Use non SNI SSL bindings for previous Windows OS (before Windows 8/2012)")]
        public bool UseCompatibilityNonSNIBindings { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class ArkMultipliersConfigSection
    {
        public ArkMultipliersConfigSection()
        {
            EggHatchSpeedMultiplier = 1d;
            BabyMatureSpeedMultiplier = 1d;
            CuddleIntervalMultiplier = 1d;
        }

        [JsonProperty(PropertyName = "eggHatchSpeedMultiplier")]
        [DisplayName("Egg Hatch Speed Multiplier")]
        [Description("Pregnancy/incubation time multiplier")]
        public double EggHatchSpeedMultiplier { get; set; }

        [JsonProperty(PropertyName = "babyMatureSpeedMultiplier")]
        [DisplayName("Baby Mature Speed Multiplier")]
        [Description("Baby mature time multiplier")]
        public double BabyMatureSpeedMultiplier { get; set; }

        [JsonProperty(PropertyName = "cuddleIntervalMultiplier")]
        [DisplayName("Cuddle Interval Multiplier")]
        [Description("Multiplier for duration between cuddles")]
        public double CuddleIntervalMultiplier { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class ServerConfigSection
    {
        public ServerConfigSection()
        {
        }

        [JsonProperty(PropertyName = "key")]
        [DisplayName("Key")]
        [Description("Unique key/name for this server instance")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "cluster")]
        [DisplayName("Cluster")]
        [Description("Optional key for the cluster instance this server is part of")]
        public string Cluster { get; set; }

        [JsonProperty(PropertyName = "saveFilePath")]
        [DisplayName("Save File Path")]
        [Description("Absolute file path of the .ark save file to monitor/extract data from")]
        public string SaveFilePath { get; set; }

        [JsonProperty(PropertyName = "displayAddress")]
        [DisplayName("Display Address")]
        [Description("Public server address visible to players")]
        public string DisplayAddress { get; set; }

        [JsonProperty(PropertyName = "ip")]
        [DisplayName("IP")]
        [Description("The IP address used to connect to this server instance")]
        public string Ip { get; set; }

        [JsonProperty(PropertyName = "queryPort")]
        [DisplayName("Query Port")]
        [Description("The port used to query to this server instance")]
        public int QueryPort { get; set; }

        [JsonProperty(PropertyName = "rconPort")]
        [DisplayName("RCON Port")]
        [Description("The port used to connect to this server instance over rcon")]
        public int RconPort { get; set; }

        [JsonProperty(PropertyName = "rconPassword")]
        [DisplayName("RCON Password")]
        [Description("The password used to connect to this server instance via rcon")]
        public string RconPassword { get; set; }

        //[JsonProperty(PropertyName = "updateBatchFilePath")]
        //[Description("Absolute file path of a batch file to run in order to update the server")]
        //public string UpdateBatchFilePath { get; set; }

        //[JsonProperty(PropertyName = "startBatchFilePath")]
        //[Description("Absolute file path of a batch file to run in order to start the server")]
        //public string StartBatchFilePath { get; set; }

        [JsonProperty(PropertyName = "serverExecutablePath")]
        [DisplayName("Server Executable Path")]
        [Description("Absolute file path of the server instance executable")]
        public string ServerExecutablePath { get; set; }

        [JsonProperty(PropertyName = "serverExecutableArguments")]
        [DisplayName("Server Executable Arguments")]
        [Description("Command line arguments used when starting the server instance")]
        public string ServerExecutableArguments { get; set; }

        [JsonProperty(PropertyName = "steamCmdExecutablePath")]
        [DisplayName("Steam Cmd Executable Path")]
        [Description("Absolute file path of the steamcmd executable")]
        public string SteamCmdExecutablePath { get; set; }

        [JsonProperty(PropertyName = "serverInstallDirPath")]
        [DisplayName("Server Install Directory Path")]
        [Description("The directory path to force steamcmd updates to")]
        public string ServerInstallDirPath { get; set; }

        [JsonProperty(PropertyName = "usePowershellOutputRedirect")]
        [DisplayName("Use Powershell Output Redirect")]
        [Description("Use alternative powershell/file based output redirect for update progress notifications")]
        public bool UsePowershellOutputRedirect { get; set; }

        [JsonProperty(PropertyName = "disableChatNotificationOnGlobalCountdown")]
        [DisplayName("Disable Chat Notification On Global Countdown")]
        [Description("Disable chat notifications for this server instance when trigged by admin multiple server countdown (feature is used for compatibility with cross server chat)")]
        public bool DisableChatNotificationOnGlobalCountdown { get; set; }
        
    }

    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class ClusterConfigSection
    {
        public ClusterConfigSection()
        {
        }

        [JsonProperty(PropertyName = "key")]
        [DisplayName("Key")]
        [Description("Unique key/name for this cluster instance")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "savePath")]
        [DisplayName("Save Path")]
        [Description("The directory path where cluster save data is stored")]
        public string SavePath { get; set; }
    }
}
