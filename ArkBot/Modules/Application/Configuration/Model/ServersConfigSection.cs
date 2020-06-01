using PropertyChanged;
using System.Collections.Generic;
using System.Linq;
using Validar;

namespace ArkBot.Modules.Application.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class ServersConfigSection : List<ServerConfigSection>
    {
        public override string ToString() => "Servers" + (Count > 0 ? $" ({string.Join(", ", this.Select(x => x.Key))})" : "");
    }
}
