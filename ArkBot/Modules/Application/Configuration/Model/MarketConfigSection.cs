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
    [DisplayName("Market Instance")]
    [CategoryOrder(ConfigurationCategory.Required, 0)]
    [CategoryOrder(ConfigurationCategory.Optional, 1)]
    [CategoryOrder(ConfigurationCategory.Advanced, 2)]
    [CategoryOrder(ConfigurationCategory.Debug, 3)]
    public class MarketConfigSection
    {
        public MarketConfigSection()
        {
        }

        public override string ToString() => MarketId;

        [JsonProperty(PropertyName = "name")]
        [Display(Name = "Name", Description = "Name")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(0)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "marketId")]
        [Display(Name = "MarketID", Description = "MarketID of the auction house.")]
        [Category(ConfigurationCategory.Required)]
        [PropertyOrder(1)]
        [MinLength(1, ErrorMessage = "{0} is not set")]
        public string MarketId { get; set; }
    }
}
