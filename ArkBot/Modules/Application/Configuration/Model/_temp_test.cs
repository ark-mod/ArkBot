﻿using ArkBot.Modules.Application.Configuration.Validation;
using Newtonsoft.Json;
using PropertyChanged;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Validar;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ArkBot.Modules.Application.Configuration.Model
{
    /// <summary>
    /// Only used for debugging the configuration system and -editor.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class Test1ConfigSection
    {
        public override string ToString() => $"Test ({(Enabled ? "Enabled" : "Disabled")})";

        public Test1ConfigSection()
        {
            Test = new Test2ConfigSection();
        }

        [JsonProperty(PropertyName = "enabled")]
        [Display(Name = "Enabled", Description = "Option to enable")]
        [PropertyOrder(0)]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "directoryPath")]
        [Display(Name = "Directory Path", Description = "Directory path")]
        [PropertyOrder(1)]
        [Editor(typeof(DirectoryPathEditor), typeof(DirectoryPathEditor))]
        [DirectoryExists(IfMethod = nameof(IsEnabled), ErrorMessage = "{0} directory path does not exist")]
        public string DirectoryPath { get; set; }

        [JsonProperty(PropertyName = "test")]
        [Display(Name = "Test", Description = "Test")]
        [PropertyOrder(2)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateExpandable(ErrorMessage = "{0} contain field(s) that are invalid")]
        public Test2ConfigSection Test { get; set; }

        // Validation methods

        private bool IsEnabled()
        {
            return Enabled;
        }
    }

    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class Test2ConfigSection
    {
        public override string ToString() => $"Test ({(Enabled ? "Enabled" : "Disabled")})";

        public Test2ConfigSection()
        {
            Test = new Test3ConfigSection();
        }

        [JsonProperty(PropertyName = "enabled")]
        [Display(Name = "Enabled", Description = "Option to enable")]
        [PropertyOrder(0)]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "directoryPath")]
        [Display(Name = "Directory Path", Description = "Directory path")]
        [PropertyOrder(1)]
        [Editor(typeof(DirectoryPathEditor), typeof(DirectoryPathEditor))]
        [DirectoryExists(IfMethod = nameof(IsEnabled), ErrorMessage = "{0} directory path does not exist")]
        public string DirectoryPath { get; set; }

        [JsonProperty(PropertyName = "test")]
        [Display(Name = "Test", Description = "Test")]
        [PropertyOrder(2)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateExpandable(ErrorMessage = "{0} contain field(s) that are invalid")]
        public Test3ConfigSection Test { get; set; }

        // Validation methods

        private bool IsEnabled()
        {
            return Enabled;
        }
    }

    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class Test3ConfigSection
    {
        public override string ToString() => $"Test ({(Enabled ? "Enabled" : "Disabled")})";

        public Test3ConfigSection()
        {
            Servers = new ServersConfigSection();
        }

        [JsonProperty(PropertyName = "enabled")]
        [Display(Name = "Enabled", Description = "Option to enable")]
        [PropertyOrder(0)]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "directoryPath")]
        [Display(Name = "Directory Path", Description = "Directory path")]
        [PropertyOrder(1)]
        [Editor(typeof(DirectoryPathEditor), typeof(DirectoryPathEditor))]
        [DirectoryExists(IfMethod = nameof(IsEnabled), ErrorMessage = "{0} directory path does not exist")]
        public string DirectoryPath { get; set; }

        [JsonProperty(PropertyName = "servers")]
        [Display(Name = "Servers", Description = "Server instance configurations")]
        [PropertyOrder(2)]
        [Editor(typeof(CustomCollectionEditor), typeof(CustomCollectionEditor))]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateCollection(ErrorMessage = "{0} contains item(s) that are invalid")]
        public ServersConfigSection Servers { get; set; }

        // Validation methods

        private bool IsEnabled()
        {
            return Enabled;
        }
    }
}
