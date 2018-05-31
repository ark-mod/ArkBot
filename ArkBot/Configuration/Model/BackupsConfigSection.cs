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
    public class BackupsConfigSection
    {
        public override string ToString() => $"Backups ({(BackupsEnabled ? "Enabled" : "Disabled")})";

        [JsonProperty(PropertyName = "backupsEnabled")]
        [Display(Name = "Backups Enabled", Description = "Option to enable savegame backups")]
        [PropertyOrder(0)]
        public bool BackupsEnabled { get; set; }

        [JsonProperty(PropertyName = "backupsDirectoryPath")]
        [Display(Name = "Backups Directory Path", Description = "Directory path where savegame backups are stored")]
        [PropertyOrder(1)]
        [Editor(typeof(DirectoryPathEditor), typeof(DirectoryPathEditor))]
        [DirectoryExists(IfMethod = nameof(IsBackupsEnabled), ErrorMessage = "{0} directory path does not exist")]
        public string BackupsDirectoryPath { get; set; }

        // Validation methods

        private bool IsBackupsEnabled()
        {
            return BackupsEnabled;
        }
    }
}
