using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArkBot.Configuration;
using ArkBot.Configuration.Validation;
using Discord;
using Microsoft.IdentityModel;
using PropertyChanged;
using Validar;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace ArkBot.Configuration.Model
{
    [AddINotifyPropertyChangedInterface]
    [InjectValidation]
    [ReadOnly(true)]
    [TypeConverter(typeof(AccessControlConfigSectionConverter<string, AccessControlFeatureGroup>))]
    [ExpandableObject]
    public class AccessControlConfigSection : Dictionary<string, AccessControlFeatureGroup>
    {
        public override string ToString() => "Access Control";
    }
}
