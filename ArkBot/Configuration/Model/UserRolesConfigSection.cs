using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PropertyChanged;
using Validar;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ArkBot.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class UserRolesConfigSection : List<UsersInRoleConfig>
    {
        public override string ToString() => "User Roles" + (Count > 0 ? $" ({(string.Join(", ", this.Select(x => x.Role)))})" : "");
    }
}
