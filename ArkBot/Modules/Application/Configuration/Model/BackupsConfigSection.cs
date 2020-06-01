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
    public class BackupsConfigSection
    {
        public override string ToString() => $"Backups ({(Enabled ? "Enabled" : "Disabled")})";

        [JsonProperty(PropertyName = "enabled")]
        [Display(Name = "Enabled", Description = "Option to enable savegame backups")]
        [PropertyOrder(0)]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "backupsDirectoryPath")]
        [Display(Name = "Backups Directory Path", Description = "Directory path where savegame backups are stored")]
        [PropertyOrder(1)]
        [Editor(typeof(DirectoryPathEditor), typeof(DirectoryPathEditor))]
        [DirectoryExists(IfMethod = nameof(IsBackupsEnabled), ErrorMessage = "{0} directory path does not exist")]
        public string BackupsDirectoryPath { get; set; }

        // Validation methods

        private bool IsBackupsEnabled()
        {
            return Enabled;
        }
    }
}
