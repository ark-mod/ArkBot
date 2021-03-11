using PropertyChanged;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Validar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using ArkBot.Modules.Application.Configuration.Validation;

namespace ArkBot.Modules.Application.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class AuctionHouseConfigSection
    {
        public AuctionHouseConfigSection()
        {
            Enabled = false;
            Markets = new MarketsConfigSection();
        }

        public override string ToString() => "Auction House";

        [JsonProperty(PropertyName = "enabled")]
        [PropertyOrder(1)]
        [Display(Name = "Enabled", Description = "Option to enable/disable the prometheus endpoint")]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "Markets")]
        [Display(Name = "Markets", Description = "MarketIDs of markets to display")]
        [PropertyOrder(2)]
        [Editor(typeof(CustomCollectionEditor), typeof(CustomCollectionEditor))]
        [Required(ErrorMessage = "{0} is not set")]
        [ValidateCollection(ErrorMessage = "{0} contains item(s) that are invalid")]
        public MarketsConfigSection Markets { get; set; }
    }
}