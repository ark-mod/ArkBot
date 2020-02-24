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
    [DisplayName("Server Instance")]
    [CategoryOrder(ConfigurationCategory.Required, 0)]
    [CategoryOrder(ConfigurationCategory.Optional, 1)]
    [CategoryOrder(ConfigurationCategory.Advanced, 2)]
    [CategoryOrder(ConfigurationCategory.Debug, 3)]
    public class ServerConfigSection
    {
        public ServerConfigSection()
        {
            ServerManagement = new ServerManagementConfigSection();
        }

        public override string ToString() => Key;

        // Required

        [JsonProperty(PropertyName = "key")]
        [Display(Name = "Key", Description = "Unique key (tag name) for this server instance")]
        [ConfigurationHelp(remarks: new [] {
            "Used to identify a particular server instance in Discord commands, Companion App etc."
        }, Example = "`server1`, `server2`, `server3`")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(0)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        [RegularExpression(@"^[a-z0-9_\-]+$", ErrorMessage = "{0} must consist of letters (a-z), numbers (0-9), dashes (-) and underscores (_)")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "saveFilePath")]
        [Display(Name = "Save File Path", Description = "The savegame (`.ark`) to extract data from and watch for changes")]
        [ConfigurationHelp(remarks: new [] { @"Savegames are found in your ARK installation folder under `ShooterGame\Saved\SavedArks\`." }, 
            Example = @"`C:\ARK Servers\server1\ShooterGame\Saved\SavedArks\TheIsland.ark`")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(1)]
        [Editor(typeof(OpenFilePathEditor), typeof(OpenFilePathEditor))]
        [OpenFilePathEditor(Filter = "ARK savegame files (*.ark)|*.ark|All files (*.*)|*.*")]
        [FileExists(ErrorMessage = "{0} is not set or the file path does not exist")]
        public string SaveFilePath { get; set; }

        [JsonProperty(PropertyName = "ip")]
        [Display(Name = "IP Address", Description = "The address used to connect to this server instance")]
        [ConfigurationHelp(remarks: new [] { "Use your servers external IP address unless you have other requirements."}, 
            Example = "`127.0.0.1`")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(2)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        public string Ip { get; set; }

        [JsonProperty(PropertyName = "queryPort")]
        [Display(Name = "Query Port", Description = "The port used to query to this server instance")]
        [ConfigurationHelp(remarks: new [] { "Use same value as `?QueryPort=` from the command used to start your server." }, Example = "`27015`")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(3)]
        [Range(1, 65535, ErrorMessage = "{0} must be between 1-65635")]
        public int QueryPort { get; set; }

        // Optional

        [JsonProperty(PropertyName = "clusterKey")]
        [Display(Name = "Cluster Key", Description = "Optional key for the cluster instance this server is part of")]
        [ConfigurationHelp(remarks: new[] { "Only used if your server is part of a cluster." }, instructions: new [] {
            @"1. Configure a cluster instance in ARK Bot.",
            @"2. Connect all servers in the cluster by specifying the unique cluster key in this field."
        }, Example = "`cluster1`")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(0)]
        [RegularExpression(@"^[a-z0-9_\-]+$", ErrorMessage = "{0} must be empty or consist of letters (a-z), numbers (0-9), dashes (-) and underscores (_)")]
        public string ClusterKey { get; set; }

        [JsonProperty(PropertyName = "modIds")]
        [Display(Name = "Mod IDs", Description = "Mods that are enabled on this server")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(1)]
        public List<int> ModIds { get; set; } = new List<int>();

        [JsonProperty(PropertyName = "displayAddress")]
        [Display(Name = "Display Address", Description = "The public address used to connect to the server")]
        [ConfigurationHelp(remarks: new [] { "Used for displaying server urls and for steam connection links." }, 
            Example = "`arkserver.net:27015`")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(2)]
        public string DisplayAddress { get; set; }

        [JsonProperty(PropertyName = "rconPort")]
        [Display(Name = "RCON Port", Description = "The port used to connect to this server instance over rcon")]
        [ConfigurationHelp(remarks: new[] { "Use same value as `?RCONPort=` from the command used to start your server." }, Example = "`27020`")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(3)]
        [RangeOptional(1, 65535, Optional = true, ErrorMessage = "{0} must be empty or between 1-65635")]
        public int? RconPort { get; set; }

        [JsonProperty(PropertyName = "rconPassword")]
        [Display(Name = "RCON Password", Description = "The password used to connect to this server instance via rcon")]
        [ConfigurationHelp(remarks: new[] { "Use same value as `?ServerAdminPassword=` from the command used to start your server." }, Example = "`password`")]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(4)]
        public string RconPassword { get; set; }

        [JsonProperty(PropertyName = "serverManagement")]
        [Display(Name = "Server Management", Description = "Configure server management features available through Discord commands")]
        [ConfigurationHelp(remarks: new[] { "Includes features like start, restart, stop, update etc." })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(5)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        public ServerManagementConfigSection ServerManagement { get; set; }


        // Advanced

        [JsonProperty(PropertyName = "disableChatNotifications")]
        [Display(Name = "Disable Chat Notifications", Description = "Option to disable chat notifications for this server instance")]
        [ConfigurationHelp(remarks: new[] { "This option is used together with ARK Server API and ARK Cross Server Chat plugin to controll which servers are notified on countdowns.\r\n",
            "By default ARK Bot will notify all servers. When using ARK Cross Server Chat only a single server should be notified since all incoming chat messages will be relayed to the rest of the cluster by the plugin." })]
        [Category(ConfigurationCategory.Advanced)]
        [PropertyOrder(0)]
        public bool DisableChatNotifications { get; set; }
    }
}
