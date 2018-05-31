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
    [DisplayName("Cluster Instance")]
    [CategoryOrder(ConfigurationCategory.Required, 0)]
    [CategoryOrder(ConfigurationCategory.Optional, 1)]
    [CategoryOrder(ConfigurationCategory.Advanced, 2)]
    [CategoryOrder(ConfigurationCategory.Debug, 3)]
    public class ClusterConfigSection
    {
        public ClusterConfigSection()
        {
        }

        public override string ToString() => Key;


        [JsonProperty(PropertyName = "key")]
        [Display(Name = "Key", Description = "Unique key/name for this cluster instance")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(0)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        [RegularExpression(@"^[a-z0-9_\-]+$", ErrorMessage = "{0} must consist of letters (a-z), numbers (0-9), dashes (-) and underscores (_)")]
        public string Key { get; set; }

        [JsonProperty(PropertyName = "savePath")]
        [Display(Name = "Save Path", Description = "The directory path where cluster save data is stored")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(1)]
        [Editor(typeof(DirectoryPathEditor), typeof(DirectoryPathEditor))]
        [DirectoryExists(ErrorMessage = "{0} directory path does not exist")]
        public string SavePath { get; set; }
    }
}
