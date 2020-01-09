using System.Collections.Generic;
using PropertyChanged;
using Validar;

namespace ArkBot.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    public class AccessControlFeatureRoles : List<string>
    {
        public AccessControlFeatureRoles() { }

        public AccessControlFeatureRoles(string[] roles) => AddRange(roles);
    }
}
