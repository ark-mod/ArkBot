using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ArkBot.Configuration
{
    public class TypeToDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Type t = (Type)value;
            var attrs = t.GetCustomAttributes(false).Cast<Attribute>().ToArray();
            var displayAttr = attrs.OfType<DisplayAttribute>().FirstOrDefault();
            var displayNameAttr = attrs.OfType<DisplayNameAttribute>().FirstOrDefault();
            return displayNameAttr?.DisplayName ?? displayAttr?.Name ?? t.Name;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
