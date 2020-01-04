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

namespace ArkBot.Configuration.Model
{
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
            UserRoles = new UserRolesConfigSection();
            ArkMultipliers = new ArkMultipliersConfigSection();
            Servers = new ServersConfigSection();
            Clusters = new ClustersConfigSection();
            WebAppRedirectListenPrefix = new string[] { };
            AccessControl = new AccessControlConfigSection();
            Discord = new DiscordConfigSection();
            WebApp = new WebAppConfigSection();
            Backups = new BackupsConfigSection();

            //Test = new Test1ConfigSection();
        }

        public void SetupDefaults()
        {
            AccessControl.SetupConfigDefaults();
        }

        // Required

        [JsonProperty(PropertyName = "steamApiKey")]
        [Display(Name = "Steam API Key", Description = "API Key from Steam used for fetching server and user details from the Steam API")]
        [ConfigurationHelp(instructions: new[] {
            @"1. Go to [Register Steam Web API Key](https://steamcommunity.com/dev/apikey) and create a new Steam API Key."},
            Example = "`5E3512AD4C7EB822105E9C07910E2A01`")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(1)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        //todo: validate steam api key with steam
        public string SteamApiKey { get; set; }

        [JsonProperty(PropertyName = "servers")]
        [Display(Name = "Servers", Description = "Server instance configurations for your particular server setup")]
        [ConfigurationHelp(remarks: new[] {
            "In this section configure servers to match your ARK server environment. Add or remove additional server configurations if necessary.\r\n",
            "Servers each have a unique key that is used to identify a particular server instance in Discord commands, Companion App etc.\r\n"
            })]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(2)]
        [Editor(typeof(CustomCollectionEditor), typeof(CustomCollectionEditor))]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateCollection(ErrorMessage = "{0} contains item(s) that are invalid")]
        public ServersConfigSection Servers { get; set; }

        [JsonProperty(PropertyName = "clusters")]
        [Display(Name = "Clusters", Description = "Cluster instance configurations for your particular server setup")]
        [ConfigurationHelp(remarks: new[] {
            "In this section configure clusters to match your ARK server environment. Add or remove additional cluster configurations if necessary.\r\n",
            "Leave it empty if your server setup does not include a cluster.\r\n",
            "Servers are connected to clusters through a unique key. Remember the key you assigned when configuring your servers.\r\n"
            })]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(3)]
        [Editor(typeof(CustomCollectionEditor), typeof(CustomCollectionEditor))]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateCollection(ErrorMessage = "{0} contains item(s) that are invalid")]
        public ClustersConfigSection Clusters { get; set; }


        
        // Optional

        [JsonProperty(PropertyName = "botName")]
        [Display(Name = "Bot Name", Description = "Short name to identify the bot")]
        [DefaultValue("ARK Bot")]
        [ConfigurationHelp(remarks: new[] { "Is used as the title and application name when communicating with third party web services etc." }, Example = "ARK Bot")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(0)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        public string BotName { get; set; }

        [JsonProperty(PropertyName = "botUrl")]
        [Display(Name = "Bot URL", Description = "Website url associated with the bot or ARK server")]
        [ConfigurationHelp(remarks: new[] { "Is used to link or redirect to your website." }, Example = "http://www.arkserver.net")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(1)]
        [ValidUrl(Optional = true, ErrorMessage = "{0} is not a valid URL")]
        public string BotUrl { get; set; }

        [JsonProperty(PropertyName = "appUrl")]
        [Display(Name = "Web App URL", Description = "External url pointing to the Web App")]
        [ConfigurationHelp(remarks: new[] { "Is used to link or redirect to the Companion App (Web App)." }, Example = "http://app.arkserver.net")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(2)]
        [ValidUrl(Optional = true, ErrorMessage = "{0} is not a valid URL")]
        public string AppUrl { get; set; }

        [JsonProperty(PropertyName = "webApp")]
        [Display(Name = "Web App", Description = "Settings specific to the Web App feature")]
        [ConfigurationHelp(remarks: new[] { "The Web App aims to provide important functions to players: dino listings, food-status, breeding info, statistics; and server admins: rcon-commands, server managing etc." })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(3)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateExpandable(ErrorMessage = "{0} contain field(s) that are invalid")]
        public WebAppConfigSection WebApp { get; set; }

        [JsonProperty(PropertyName = "discord")]
        [Display(Name = "Discord", Description = "Discord bot settings")]
        [ConfigurationHelp(remarks: new[] { "Settings specific to the Discord bot feature. The Discord bot allows administrators to manage servers using commands in Discord." })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(4)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateExpandable(ErrorMessage = "{0} contain field(s) that are invalid")]
        public DiscordConfigSection Discord { get; set; }

        [JsonProperty(PropertyName = "arkMultipliers")]
        [Display(Name = "ARK Multipliers", Description = "Server specific multipliers")]
        [ConfigurationHelp(remarks: new[] { "ARK configuration multipliers used on your servers and required for accurate calculations throughout the application." })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(5)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateExpandable(ErrorMessage = "{0} contain field(s) that are invalid")]
        public ArkMultipliersConfigSection ArkMultipliers { get; set; }

        [JsonProperty(PropertyName = "userRoles")]
        [Display(Name = "User Roles", Description = "Explicit steam user role assignment")]
        [ConfigurationHelp(remarks: new[] { "Multiple roles can be configured and each contains a list of steam ids who belong to that role. Roles are used in Companion App (Web App) access control to grant access to specific pages/features." })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(6)]
        [Editor(typeof(CustomCollectionEditor), typeof(CustomCollectionEditor))]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateCollection(ErrorMessage = "{0} contains item(s) that are invalid")]
        public UserRolesConfigSection UserRoles { get; set; }

        [JsonProperty(PropertyName = "accessControl")]
        [Display(Name = "Access Control", Description = "Per-feature role based access control configuration")]
        [ConfigurationHelp(remarks: new[] { "Contain a predefined set of pages/features to grant access to. Each page/feature contains a list of roles that have access to that particular resource. Roles are connected to steam users in the User Roles setting." })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(7)]
        [Required(ErrorMessage = "{0} is not set")]
        public AccessControlConfigSection AccessControl { get; set; }

        [JsonProperty(PropertyName = "backups")]
        [Display(Name = "Backups", Description = "Savegame backups")]
        [ConfigurationHelp(remarks: new[] { "Settings specific to the savegame backup feature which optionally can be configured to take backups each time a savegame change is detected." })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(8)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateExpandable(ErrorMessage = "{0} contain field(s) that are invalid")]
        public BackupsConfigSection Backups { get; set; }

        [JsonProperty(PropertyName = "webAppRedirectListenPrefix")]
        [Display(Name = "Web App Redirect Listen Prefix(es)", Description = "Http listen prefix(es) that are redirected to BotUrl")]
        [ConfigurationHelp(remarks: new[] { "Used to redirect alternate URLs to the actual Companion App (Web App) URL. Typically used to redirect HTTP requests to a secure HTTPS connection when SSL is enabled." }, Example = "http://+:80/")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(9)]
        //todo: validate this listen prefix
        public string[] WebAppRedirectListenPrefix { get; set; }

        [JsonProperty(PropertyName = "powershellFilePath")]
        [Display(Name = "Powershell Executable Path", Description = "Absolute file path of the powershell executable")]
        [DefaultValue(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe")]
        [ConfigurationHelp(remarks: new[] { "This is the path to the PowerShell executable on your system. It is used when `Use Powershell Output Redirect` is configured for a server instance to relay SteamCmd status back to ARK Bot." })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(10)]
        [Editor(typeof(OpenFilePathEditor), typeof(OpenFilePathEditor))]
        [OpenFilePathEditor(Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*")]
        [FileExists(ErrorMessage = "{0} is not set or the file path does not exist")]
        //todo: only used with Server.ServerManagement.UsePowershellOutputRedirect
        public string PowershellFilePath { get; set; }

        [JsonProperty(PropertyName = "ssl")]
        [Display(Name = "SSL", Description = "Configure Web App and WebAPI to use SSL with a free certificate from Lets Encrypt")]
        [ConfigurationHelp(remarks: new[]
        {
            "Enabling SSL (HTTPS) is important to protect users against session hijacking. With SSL enabled the bot will attempt to issue a free SSL-certificate using Lets Encrypt. Once the SSL-certificate has been issued, it is valid for 90 days, after which it must be renewed.\r\n",
            "For the SSL-certificate to be issued you need to prove that you control the domain name; the domain must be pointed to your public IP and port 80 must be open externally and available for the bot to bind locally.\r\n"
        })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(11)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateExpandable(ErrorMessage = "{0} contain field(s) that are invalid")]
        public SslConfigSection Ssl { get; set; }

        [JsonProperty(PropertyName = "webApiListenPrefix")]
        [Display(Name = "Web API Listen Prefix", Description = "Http listen prefix assigned to the Web API")]
        [DefaultValue("http://+:60001/")]
        [ConfigurationHelp(remarks: new[] {
            "Listen prefix is a simple canonical form to express the schema, host, port and relative URI to reserve for a web listener.\r\n",
            "Typically the primary thing you might change is the port which is the decimal number after `:`. This is the local port that the listener will attempt to bind to.\r\n",
            "With SSL is enabled the schema should be changed from `http` to `https`.\r\n",
            "[Learn more about listen prefixes](https://msdn.microsoft.com/en-us/library/windows/desktop/aa364698(v=vs.85).aspx)\r\n"
        })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(12)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        //todo: validate this listen prefix
        [RegularExpressionCustom(@"^https://.*", IfMethod = nameof(IsSslEnabled), ErrorMessage = "{0} should be `https` when SSL is enabled")]
        [RegularExpressionCustom(@"^http://.*", IfMethod = nameof(IsSslDisabled), ErrorMessage = "{0} should be `http` when SSL is disabled")]

        public string WebApiListenPrefix { get; set; }

        [JsonProperty(PropertyName = "webAppListenPrefix")]
        [Display(Name = "Web App Listen Prefix", Description = "Http listen prefix assigned to the Companion App (Web App)")]
        [DefaultValue("http://+:80/")]
        [ConfigurationHelp(remarks: new [] {
            "Listen prefix is a simple canonical form to express the schema, host, port and relative URI to reserve for a web listener.\r\n",
            "Typically the primary thing you might change is the port which is the decimal number after `:`. This is the local port that the listener will attempt to bind to.\r\n",
            "With SSL is enabled the schema should be changed from `http` to `https`.\r\n",
            "[Learn more about listen prefixes](https://msdn.microsoft.com/en-us/library/windows/desktop/aa364698(v=vs.85).aspx)\r\n"
        })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(13)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        //todo: validate this listen prefix
        [RegularExpressionCustom(@"^https://.*", IfMethod = nameof(IsSslEnabled), ErrorMessage = "{0} should be `https` when SSL is enabled")]
        [RegularExpressionCustom(@"^http://.*", IfMethod = nameof(IsSslDisabled), ErrorMessage = "{0} should be `http` when SSL is disabled")]
        public string WebAppListenPrefix { get; set; }

        [JsonProperty(PropertyName = "tempFileOutputDirPath")]
        [Display(Name = "Temporary Files Directory", Description = "An existing directory path where temporary binary files can be stored (zip-files etc.)")]
        [DefaultValue("%TEMP%\\ArkBot")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(14)]
        [Editor(typeof(DirectoryPathEditor), typeof(DirectoryPathEditor))]
        [DirectoryPathIsValid(ErrorMessage = "{0} is not set or the directory path is not valid")]
        public string TempFileOutputDirPath { get; set; }

        [JsonProperty(PropertyName = "hideUiOnStartup")]
        [Display(Name = "Hide Ui On Startup", Description = "Hides the user interface on program startup")]
        [DefaultValue(false)]
        [ConfigurationHelp(remarks: new[] { "Allows hiding the user interface on program startup. The program can be accessed from the system tray icon." })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(15)]
        public bool HideUiOnStartup { get; set; }


        // Optional Advanced

        [JsonProperty(PropertyName = "savegameExtractionMaxDegreeOfParallelism")]
        [Display(Name = "Savegame Extraction Max Degree Of Parallelism", Description = "Max degree of parallelism to use for savegame extraction")]
        [DefaultValue(null)]
        [ConfigurationHelp(remarks: new [] { "No need to change this setting unless you are experiencing out of memory exceptions." })]
        [Category(ConfigurationCategory.Advanced)]
        [PropertyOrder(0)]
        [RangeOptional(1, 32, Optional = true, ErrorMessage = "{0} must be empty or between 1-32")]
        public int? SavegameExtractionMaxDegreeOfParallelism { get; set; }

        [JsonProperty(PropertyName = "useCompatibilityChangeWatcher")]
        [Display(Name = "Use Compatibility Change Watcher", Description = "Use timer based .ark save file watcher rather than the default (based on FileSystemWatcher)")]
        [DefaultValue(true)]
        [ConfigurationHelp(remarks: new[] { "An alternative method of watching for file changes that offer better compatibility with a wide range of systems." })]
        [Category(ConfigurationCategory.Advanced)]
        [PropertyOrder(1)]
        public bool UseCompatibilityChangeWatcher { get; set; }


        // Debugging, Logging etc.

        [JsonProperty(PropertyName = "discordLogLevel")]
        [Display(Name = "Discord Log Level", Description = "Log level for Discord.NET")]
        [Category(ConfigurationCategory.Debug)]
        [PropertyOrder(0)]
        public LogSeverity DiscordLogLevel { get; set; }

        [JsonProperty(PropertyName = "anonymizeWebApiData")]
        [Display(Name = "Anonymize Web API Data", Description = "Anonymize all data in the WebAPI. Used to create data dumps for demoing the web-app")]
        [Category(ConfigurationCategory.Debug)]
        [PropertyOrder(1)]
        public bool AnonymizeWebApiData { get; set; }

        //[JsonProperty(PropertyName = "test")]
        //[Display(Name = "Test", Description = "Test")]
        //[Category(ConfigurationCategory.Debug)]
        //[PropertyOrder(2)]
        //[ExpandableObject]
        //[Required(ErrorMessage = "{0} is not set")]
        //[ValidateExpandable(ErrorMessage = "{0} contain field(s) that are invalid")]
        //public Test1ConfigSection Test { get; set; }


        // Validation methods

        private bool IsSslEnabled()
        {
            return Ssl.Enabled;
        }

        private bool IsSslDisabled()
        {
            return !Ssl.Enabled;
        }
    }
}
