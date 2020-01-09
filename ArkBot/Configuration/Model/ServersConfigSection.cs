using System.Collections.Generic;
using System.Linq;
using PropertyChanged;
using Validar;

namespace ArkBot.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class ServersConfigSection : List<ServerConfigSection>
    {
        public override string ToString() => "Servers" + (Count > 0 ? $" ({(string.Join(", ", this.Select(x => x.Key)))})" : "");
    }
}
