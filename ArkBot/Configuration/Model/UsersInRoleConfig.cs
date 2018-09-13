using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ArkBot.Configuration.Validation;
using Newtonsoft.Json;
using PropertyChanged;
using Validar;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ArkBot.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    [DisplayName("User Role")]
    [CategoryOrder(ConfigurationCategory.Required, 0)]
    [CategoryOrder(ConfigurationCategory.Optional, 1)]
    [CategoryOrder(ConfigurationCategory.Advanced, 2)]
    [CategoryOrder(ConfigurationCategory.Debug, 3)]
    public class UsersInRoleConfig
    {
        public UsersInRoleConfig()
        {
            SteamIds = new List<string>();
        }

        public override string ToString() => Role;

        [JsonProperty(PropertyName = "role")]
        [Display(Name = "Role", Description = "Name of the role")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(1)]
        [MinLengthOptional(1, ErrorMessage = "{0} is not set")]
        public string Role { get; set; }

        [JsonProperty(PropertyName = "steamIds")]
        [Display(Name = "Steam IDs", Description = "Steam IDs that are assigned to this role")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(2)]
        [Required(ErrorMessage = "{0} is not set")]
        public List<string> SteamIds { get; set; }
    }
}
