using PropertyChanged;
using System.Collections.Generic;
using System.Linq;
using Validar;

namespace ArkBot.Modules.Application.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class UserRolesConfigSection : List<UsersInRoleConfig>
    {
        public override string ToString() => "User Roles" + (Count > 0 ? $" ({string.Join(", ", this.Select(x => x.Role))})" : "");
    }
}
