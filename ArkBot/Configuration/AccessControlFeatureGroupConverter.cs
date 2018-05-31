using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ArkBot.Configuration
{
    public class AccessControlFeatureGroupConverter<T, T2> : ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (!(value is IDictionary<T, T2>)) return base.GetProperties(context, value, attributes);

            var dict = value as IDictionary<T, T2>;
            var propDescriptions = new PropertyDescriptorCollection(null);

            var num = 0;
            foreach (var kv in dict) propDescriptions.Add(new AccessControlFeaturePropertyDescriptor<T, T2>(dict, kv.Key, num++));
            return propDescriptions;
        }
    }
}
