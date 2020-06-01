using PropertyChanged;
using System.Collections.Generic;
using Validar;

namespace ArkBot.Modules.Application.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class AccessControlFeatureRoles : List<string>
    {
        public AccessControlFeatureRoles() { }

        public AccessControlFeatureRoles(string[] roles) => AddRange(roles);
    }
}
