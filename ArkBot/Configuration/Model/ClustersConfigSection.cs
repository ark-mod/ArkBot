using System.Collections.Generic;
using System.Linq;
using PropertyChanged;
using Validar;

namespace ArkBot.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class ClustersConfigSection : List<ClusterConfigSection>
    {
        public override string ToString() => "Clusters" + (Count > 0 ? $" ({(string.Join(", ", this.Select(x => x.Key)))})" : "");
    }
}
