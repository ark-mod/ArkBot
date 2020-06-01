using PropertyChanged;
using System.Collections.Generic;
using System.ComponentModel;
using Validar;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ArkBot.Modules.Application.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    [TypeConverter(typeof(AccessControlFeatureGroupConverter<string, AccessControlFeatureRoles>))]
    [ExpandableObject]
    public class AccessControlFeatureGroup : Dictionary<string, AccessControlFeatureRoles>
    {
        public override string ToString() => "Group";

        internal void SetupDefaults(string[] keys)
        {
            foreach (var key in keys)
                if (!ContainsKey(key)) Add(key, new AccessControlFeatureRoles(new[] { "" }));
        }
    }
}
