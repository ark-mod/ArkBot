using ArkBot.Modules.Application.Configuration.Validation;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ArkBot.Modules.Application.Configuration.Model
{
    public enum WebAppTheme { Dark = 0, Light = 1 };

    public class WebAppConfigSection
    {
        public WebAppConfigSection()
        {
            Ssl = new SslConfigSection();
            UserRoles = new UserRolesConfigSection();
            AccessControl = new AccessControlConfigSection();
        }

        public override string ToString() => $"Web App";

        [JsonProperty(PropertyName = "externalUrl")]
        [Display(Name = "External URL", Description = "External url pointing to the Web App")]
        [ConfigurationHelp(remarks: new[] { "Is used to link or redirect to the Companion App (Web App)." }, Example = "http://app.arkserver.net")]
        //[Category(ConfigurationCategory.Optional)]
        [PropertyOrder(1)]
        [ValidUrl(Optional = true, ErrorMessage = "{0} is not a valid URL")]
        public string ExternalUrl { get; set; }

        [JsonProperty(PropertyName = "iPEndpoint")]
        [Display(Name = "IP Endpoint", Description = "IP Endpoint assigned to the Companion App (Web App)")]
        [DefaultValue("0.0.0.0:80")]
        [ConfigurationHelp(remarks: new[] {
            "Represents a network endpoint as an IP address and a port number.\r\n",
            "Typically the primary thing you might change is the port which is the decimal number after `:`. This is the local port that the listener will attempt to bind to.\r\n",
        })]
        //[Category(ConfigurationCategory.Optional)]
        [PropertyOrder(2)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        //todo: validate this listen prefix
        public string IPEndpoint { get; set; }

        [JsonProperty(PropertyName = "ssl")]
        [Display(Name = "SSL", Description = "Configure Web App and WebAPI to use SSL with a free certificate from Lets Encrypt")]
        [ConfigurationHelp(remarks: new[]
{
            "Enabling SSL (HTTPS) is important to protect users against session hijacking. With SSL enabled the bot will attempt to issue a free SSL-certificate using Lets Encrypt. Once the SSL-certificate has been issued, it is valid for 90 days, after which it must be renewed.\r\n",
            "For the SSL-certificate to be issued you need to prove that you control the domain name; the domain must be pointed to your public IP and port 80 must be open externally and available for the bot to bind locally.\r\n"
        })]
        [Category(ConfigurationCategory.Optional)]
        [PropertyOrder(3)]
        [ExpandableObject]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateExpandable(ErrorMessage = "{0} contain field(s) that are invalid")]
        public SslConfigSection Ssl { get; set; }

        [JsonProperty(PropertyName = "defaultTheme")]
        [Display(Name = "Default Theme", Description = "Default theme to use in the Web App")]
        [PropertyOrder(4)]
        public WebAppTheme DefaultTheme { get; set; } = WebAppTheme.Dark;

        [JsonProperty(PropertyName = "topMenu")]
        [Display(Name = "Top Menu", Description = "Use top menu in the Web App")]
        [PropertyOrder(5)]
        public bool TopMenu { get; set; } = false;

        [JsonProperty(PropertyName = "tribeLogLimit")]
        [Display(Name = "Tribe Log Limit", Description = "Limit for how many tribe logs are displayed in the Web App")]
        [PropertyOrder(6)]
        [RangeOptional(1, 1000, Optional = false, ErrorMessage = "{0} must be between 1-1000")]
        public int TribeLogLimit { get; set; } = 100;

        [JsonProperty(PropertyName = "tribeLogColors")]
        [Display(Name = "Tribe Log Colors", Description = "Enable colored tribe log entries in the Web App")]
        [PropertyOrder(7)]
        public bool TribeLogColors { get; set; } = false;

        [JsonProperty(PropertyName = "customCssFilePath")]
        [Display(Name = "Custom Style Sheet Path", Description = "Path to a custom style sheet file (.css) to use in the Web App")]
        [ConfigurationHelp(remarks: new[] {
            "The Web App supports creating custom themes and importing them as a style sheet file (.css).\r\n",
            "Open the Custom Theme Creator in the Web App from the `Admin Options` menu accessed using the hotkey `Shift + Control + A`.",
            "After customizing your theme paste the generated styles into a new style sheet file and set the file path in this property.",
        }, Example = @"`C:\ARKBot\WebApp\custom.css`")]
        [PropertyOrder(8)]
        [Editor(typeof(OpenFilePathEditor), typeof(OpenFilePathEditor))]
        [OpenFilePathEditor(Filter = "Style Sheet files (*.css)|*.css|All files (*.*)|*.*")]
        [FileExists(ErrorMessage = "{0} is not set or the file path does not exist")]
        public string CustomCssFilePath { get; set; }

        [JsonProperty(PropertyName = "userRoles")]
        [Display(Name = "User Roles", Description = "Explicit steam user role assignment")]
        [ConfigurationHelp(remarks: new[] { "Multiple roles can be configured and each contains a list of steam ids who belong to that role. Roles are used in Companion App (Web App) access control to grant access to specific pages/features." })]
        //[Category(ConfigurationCategory.Optional)]
        [PropertyOrder(9)]
        [Editor(typeof(CustomCollectionEditor), typeof(CustomCollectionEditor))]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateCollection(ErrorMessage = "{0} contains item(s) that are invalid")]
        public UserRolesConfigSection UserRoles { get; set; }

        [JsonProperty(PropertyName = "accessControl")]
        [Display(Name = "Access Control", Description = "Per-feature role based access control configuration")]
        [ConfigurationHelp(remarks: new[] { "Contain a predefined set of pages/features to grant access to. Each page/feature contains a list of roles that have access to that particular resource. Roles are connected to steam users in the User Roles setting." })]
        //[Category(ConfigurationCategory.Optional)]
        [PropertyOrder(10)]
        [Required(ErrorMessage = "{0} is not set")]
        public AccessControlConfigSection AccessControl { get; set; }
    }
}
