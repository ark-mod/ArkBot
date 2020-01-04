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
    public enum WebAppTheme { Dark = 0, Light = 1 };

    public class WebAppConfigSection
    {
        public WebAppConfigSection()
        {
        }

        public override string ToString() => $"Web App";

        [JsonProperty(PropertyName = "defaultTheme")]
        [Display(Name = "Default Theme", Description = "Default theme to use in the Web App")]
        public WebAppTheme DefaultTheme { get; set; } = WebAppTheme.Dark;

        [JsonProperty(PropertyName = "tribeLogLimit")]
        [Display(Name = "Tribe Log Limit", Description = "Limit for how many tribe logs are displayed in the Web App")]
        [RangeOptional(1, 1000, Optional = false, ErrorMessage = "{0} must be between 1-1000")]
        public int TribeLogLimit { get; set; } = 100;

        [JsonProperty(PropertyName = "tribeLogColors")]
        [Display(Name = "Tribe Log Colors", Description = "Enable colored tribe log entries in the Web App")]
        public bool TribeLogColors { get; set; } = false;
    }
}
