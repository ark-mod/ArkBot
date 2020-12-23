using PropertyChanged;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Validar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ArkBot.Modules.Application.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class MarketsConfigSection : List<MarketConfigSection>
    {
        public override string ToString() => "Markets" + (Count > 0 ? $" ({string.Join(", ", this.Select(x => x.MarketId))})" : "");
    }
}