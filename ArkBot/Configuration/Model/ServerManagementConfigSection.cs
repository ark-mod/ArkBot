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
    public class ServerManagementConfigSection
    {
        public override string ToString() => $"Server Management ({(Enabled ? "Enabled" : "Disabled")})";

        [JsonProperty(PropertyName = "enabled")]
        [Display(Name = "Enabled", Description = "Option to enable server management features for this server instance")]
        [DefaultValue(true)]
        [PropertyOrder(0)]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "serverExecutablePath")]
        [Display(Name = "Server Executable Path", Description = "Path of the server executable (`ShooterGameServer.exe`)")]
        [ConfigurationHelp(remarks: new[] { @"Normally found in your ARK installation folder under `ShooterGame\Binaries\Win64\`." },
            Example = @"`C:\ARK Servers\server1\ShooterGame\Binaries\Win64\ShooterGameServer.exe`")]
        [PropertyOrder(1)]
        [Editor(typeof(OpenFilePathEditor), typeof(OpenFilePathEditor))]
        [OpenFilePathEditor(Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*")]
        [FileExists(IfMethod = nameof(IsEnabled), ErrorMessage = "{0} file path does not exist")]
        public string ServerExecutablePath { get; set; }

        [JsonProperty(PropertyName = "serverExecutableArguments")]
        [Display(Name = "Server Executable Arguments", Description = "Command line arguments used when starting the server instance")]
        [ConfigurationHelp(remarks: new[] { "Make sure to include `-serverkey=server1` and to set it to match your own  unique server key. It is used by the server management features to know which `ShooterGameServer.exe` process is which when multiple servers are hosted on the same PC." }, 
            Example = @"`TheIsland?listen?Port=7777?QueryPort=27015?RCONPort=27020?RCONEnabled=True?SessionName=Server1?ServerPassword=?ServerAdminPassword=password?SpectatorPassword=password?MaxPlayers=5 -culture=en -nosteamclient -clusterid=cluster1 -ClusterDirOverride=""C:\ARK Servers\cluster1"" -serverkey=server1`")]
        [PropertyOrder(2)]
        [MinLengthOptional(1, IfMethod = nameof(IsEnabled), ErrorMessage = "{0} is not set")]
        public string ServerExecutableArguments { get; set; }

        [JsonProperty(PropertyName = "steamCmdExecutablePath")]
        [Display(Name = "Steam Cmd Executable Path", Description = "Path of the steamcmd executable (`SteamCMD.exe`)")]
        [ConfigurationHelp(remarks: new[] { @"The Steam Console Client or SteamCMD is a command-line version of the Steam client. It is used update Steam servers and mods." },
            Example = @"`C:\SteamCMD\SteamCMD.exe`")]
        [PropertyOrder(3)]
        [Editor(typeof(OpenFilePathEditor), typeof(OpenFilePathEditor))]
        [OpenFilePathEditor(Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*")]
        [FileExists(IfMethod = nameof(IsEnabled), ErrorMessage = "{0} is not set or the file path does not exist")]
        public string SteamCmdExecutablePath { get; set; }

        [JsonProperty(PropertyName = "serverInstallDirPath")]
        [Display(Name = "Server Install Directory Path", Description = "Path of the server installation directory")]
        [ConfigurationHelp(remarks: new[] { @"Used in SteamCMD update operations as the parameter for `force_install_dir`." }, 
            Example = @"`C:\ARK Servers\server1`")]
        [PropertyOrder(4)]
        [Editor(typeof(DirectoryPathEditor), typeof(DirectoryPathEditor))]
        [DirectoryExists(IfMethod = nameof(IsEnabled), ErrorMessage = "{0} directory path does not exist")]
        public string ServerInstallDirPath { get; set; }

        [JsonProperty(PropertyName = "usePowershellOutputRedirect")]
        [Display(Name = "Use PowerShell Output Redirect", Description = "Use alternative PowerShell/file based output redirect for update progress notifications")]
        [DefaultValue(true)]
        [ConfigurationHelp(remarks: new[] { "Compatibility option to pipe SteamCMD outout through PowerShell rather than redirecting the standard output.\r\n",
            "Works on most servers - compared to standard output redirect which only works on some."})]
        [PropertyOrder(5)]
        public bool UsePowershellOutputRedirect { get; set; }


        // Validation methods

        private bool IsEnabled()
        {
            return Enabled;
        }
    }
}
