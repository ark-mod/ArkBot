using System;
using System.Collections.Generic;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.ComponentModel;

namespace ArkBot.Configuration
{
    public class AccessControlFeaturePropertyDescriptor<T, T2> : PropertyDescriptor
    {
        private readonly IDictionary<T, T2> _owner;
        private readonly T _index;
        private readonly int _order;

        public AccessControlFeaturePropertyDescriptor(IDictionary<T, T2> owner, T index, int order) : base("[" + index + "]", null)
        {
            _owner = owner;
            _index = index;
            _order = order;
        }

        public override AttributeCollection Attributes
        {
            get
            {
                var attributes = TypeDescriptor.GetAttributes(GetValue(null), false);

                attributes = AddAttribute(new DisplayNameAttribute(_index.ToString()), attributes);
                attributes = AddAttribute(new PropertyOrderAttribute(_order), attributes);
                attributes = AddAttribute(new EditorAttribute(typeof(StringArrayEditorWithPreview), typeof(StringArrayEditorWithPreview)), attributes);

                return attributes;
            }
        }
        private AttributeCollection AddAttribute(Attribute newAttribute, AttributeCollection oldAttributes)
        {
            var newAttributes = new Attribute[oldAttributes.Count + 1];
            oldAttributes.CopyTo(newAttributes, 1);
            newAttributes[0] = newAttribute;

            return new AttributeCollection(newAttributes);
        }

        public override bool CanResetValue(object component) => false;
        public override object GetValue(object component) => Value;
        private T2 Value => _owner[_index];
        public override void ResetValue(object component) => throw new NotImplementedException();
        public override void SetValue(object component, object value) => _owner[_index] = (T2)value;
        public override bool ShouldSerializeValue(object component) => false;
        public override Type ComponentType => _owner.GetType();
        public override bool IsReadOnly => false;
        public override Type PropertyType => Value?.GetType();
    }
}
