using ArkBot.Modules.Application.Configuration.Validation;
using Newtonsoft.Json;
using PropertyChanged;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Validar;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ArkBot.Modules.Application.Configuration.Model
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
            Servers = new ServersConfigSection();
            Clusters = new ClustersConfigSection();
            Discord = new DiscordConfigSection();
            WebApp = new WebAppConfigSection();
            Backups = new BackupsConfigSection();

            //Test = new Test1ConfigSection();
        }

        public void SetupDefaults()
        {
            WebApp.AccessControl.SetupConfigDefaults();
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

        [JsonProperty(PropertyName = "databaseConnectionString")]
        [Display(Name = "Database Connection String", Description = "SQL Server database connection string")]
        [DefaultValue(@"Data Source=.\SQLExpress;Integrated Security=True;Database=ArkBot")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(1)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        public string DatabaseConnectionString { get; set; }

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

        [JsonProperty(PropertyName = "backups")]
        [Display(Name = "Backups", Description = "Savegame backups")]
        [ConfigurationHelp(remarks: new[] { "Settings specific to the savegame backup feature which optionally can be configured to take backups each time a savegame change is detected." })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(8)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateExpandable(ErrorMessage = "{0} contain field(s) that are invalid")]
        public BackupsConfigSection Backups { get; set; }

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

        [JsonProperty(PropertyName = "tempFileOutputDirPath")]
        [Display(Name = "Temporary Files Directory", Description = "An existing directory path where temporary binary files can be stored (zip-files etc.)")]
        [DefaultValue("%TEMP%\\ArkBot")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(13)]
        [Editor(typeof(DirectoryPathEditor), typeof(DirectoryPathEditor))]
        [DirectoryPathIsValid(ErrorMessage = "{0} is not set or the directory path is not valid")]
        public string TempFileOutputDirPath { get; set; }


        // Optional Advanced

        [JsonProperty(PropertyName = "useCompatibilityChangeWatcher")]
        [Display(Name = "Use Compatibility Change Watcher", Description = "Use timer based .ark save file watcher rather than the default (based on FileSystemWatcher)")]
        [DefaultValue(true)]
        [ConfigurationHelp(remarks: new[] { "An alternative method of watching for file changes that offer better compatibility with a wide range of systems." })]
        [Category(ConfigurationCategory.Advanced)]
        [PropertyOrder(1)]
        public bool UseCompatibilityChangeWatcher { get; set; }


        // Debugging, Logging etc.

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
    }
}
