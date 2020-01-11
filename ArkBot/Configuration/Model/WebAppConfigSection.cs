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
        [PropertyOrder(0)]
        public WebAppTheme DefaultTheme { get; set; } = WebAppTheme.Dark;

        [JsonProperty(PropertyName = "topMenu")]
        [Display(Name = "Top Menu", Description = "Use top menu in the Web App")]
        [PropertyOrder(1)]
        public bool TopMenu { get; set; } = false;

        [JsonProperty(PropertyName = "tribeLogLimit")]
        [Display(Name = "Tribe Log Limit", Description = "Limit for how many tribe logs are displayed in the Web App")]
        [PropertyOrder(2)]
        [RangeOptional(1, 1000, Optional = false, ErrorMessage = "{0} must be between 1-1000")]
        public int TribeLogLimit { get; set; } = 100;

        [JsonProperty(PropertyName = "tribeLogColors")]
        [Display(Name = "Tribe Log Colors", Description = "Enable colored tribe log entries in the Web App")]
        [PropertyOrder(3)]
        public bool TribeLogColors { get; set; } = false;

        [JsonProperty(PropertyName = "customCssFilePath")]
        [Display(Name = "Custom Style Sheet Path", Description = "Path to a custom style sheet file (.css) to use in the Web App")]
        [ConfigurationHelp(remarks: new[] {
            "The Web App supports creating custom themes and importing them as a style sheet file (.css).\r\n",
            "Open the Custom Theme Creator in the Web App from the `Admin Options` menu accessed using the hotkey `Shift + Control + A`.",
            "After customizing your theme paste the generated styles into a new style sheet file and set the file path in this property.",
        }, Example = @"`C:\ARKBot\WebApp\custom.css`")]
        [PropertyOrder(4)]
        [Editor(typeof(OpenFilePathEditor), typeof(OpenFilePathEditor))]
        [OpenFilePathEditor(Filter = "Style Sheet files (*.css)|*.css|All files (*.*)|*.*")]
        [FileExists(ErrorMessage = "{0} is not set or the file path does not exist")]
        public string CustomCssFilePath { get; set; }
    }
}
