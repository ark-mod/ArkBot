using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace ArkBot.Configuration
{
    [ReadOnly(true)]
    [TypeConverter(typeof(MyCustomTypeDescriptor<string, AccessControlFeatureGroup>))]
    [ExpandableObject]
    public class AccessControlConfigSection : Dictionary<string, AccessControlFeatureGroup>
    {
        
    }

    [TypeConverter(typeof(MyCustomTypeDescriptor2<string, List<string>>))]
    [ExpandableObject]
    public class AccessControlFeatureGroup : Dictionary<string, List<string>>
    {

    }

    public class StringArrayEditorWithPreview : PrimitiveTypeCollectionEditor2
    {
        protected override void SetControlProperties(PropertyItem propertyItem)
        {
            base.SetControlProperties(propertyItem);
            Editor.Content = null;
        }

        public override FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            var editor = base.ResolveEditor(propertyItem);
            return editor;
        }
    }

    public class PrimitiveTypeCollectionEditor2 : TypeEditor<PrimitiveTypeCollectionControl2>
    {
        protected override void SetControlProperties(PropertyItem propertyItem)
        {
            Editor.BorderThickness = new System.Windows.Thickness(0);
        }

        protected override void SetValueDependencyProperty()
        {
            ValueProperty = PrimitiveTypeCollectionControl2.ItemsSourceProperty;
        }

        protected override void ResolveValueBinding(PropertyItem propertyItem)
        {
            var type = propertyItem.PropertyType;
            Editor.ItemsSourceType = type;

            if (type.BaseType == typeof(System.Array))
            {
                Editor.ItemType = type.GetElementType();
            }
            else
            {
                var typeArguments = type.GetGenericArguments();
                if (typeArguments.Length > 0)
                {
                    Editor.ItemType = typeArguments[0];
                }
            }

            base.ResolveValueBinding(propertyItem);
        }
    }

    public class PrimitiveTypeCollectionControl2 : PrimitiveTypeCollectionControl
    {
        protected override void OnTextChanged(string oldValue, string newValue)
        {
            base.OnTextChanged(oldValue, newValue);
            Content = newValue?.Replace("\r\n", "; ");
        }
    }

    public class MyCustomTypeDescriptor<T, T2> : ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (value is IDictionary<T, T2>)
            {
                IDictionary<T, T2> list = value as IDictionary<T, T2>;
                PropertyDescriptorCollection propDescriptions = new PropertyDescriptorCollection(null);

                var num = 0;
                foreach (var kv in list)
                {
                    propDescriptions.Add(new ListItemPropertyDescriptor<T, T2>(list, kv.Key, num++));
                }
                //IEnumerator enumerator = list.GetEnumerator();
                //int counter = -1;
                //while (enumerator.MoveNext())
                //{
                //    counter++;


                //}
                return propDescriptions;
            }
            else
            {
                return base.GetProperties(context, value, attributes);
            }
        }
    }

    public class ListItemPropertyDescriptor<T, T2> : PropertyDescriptor
    {
        private readonly IDictionary<T, T2> owner;
        private readonly T index;
        private readonly int order;

        public ListItemPropertyDescriptor(IDictionary<T, T2> owner, T index, int order) : base("[" + index + "]", null)
        {
            this.owner = owner;
            this.index = index;
            this.order = order;
        }

        public override AttributeCollection Attributes
        {
            get
            {
                var attributes = TypeDescriptor.GetAttributes(GetValue(null), false);
                //If the Xceed expandable object attribute is not applied then apply it
                //if (!attributes.OfType<ExpandableObjectAttribute>().Any())
                //{
                //    attributes = AddAttribute(new ExpandableObjectAttribute(), attributes);
                //}

                //set the xceed order attribute
                attributes = AddAttribute(new DisplayNameAttribute(index.ToString()), attributes);
                attributes = AddAttribute(new PropertyOrderAttribute(order), attributes);

                return attributes;
            }
        }
        private AttributeCollection AddAttribute(Attribute newAttribute, AttributeCollection oldAttributes)
        {
            Attribute[] newAttributes = new Attribute[oldAttributes.Count + 1];
            oldAttributes.CopyTo(newAttributes, 1);
            newAttributes[0] = newAttribute;

            return new AttributeCollection(newAttributes);
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return Value;
        }

        private T2 Value
          => owner[index];

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            owner[index] = (T2)value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType
          => owner.GetType();

        public override bool IsReadOnly
          => true;

        public override Type PropertyType
          => Value?.GetType();

    }

    public class MyCustomTypeDescriptor2<T, T2> : ExpandableObjectConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (value is IDictionary<T, T2>)
            {
                IDictionary<T, T2> list = value as IDictionary<T, T2>;
                PropertyDescriptorCollection propDescriptions = new PropertyDescriptorCollection(null);

                var num = 0;
                foreach (var kv in list)
                {
                    propDescriptions.Add(new ListItemPropertyDescriptor2<T, T2>(list, kv.Key, num++));
                }
                //IEnumerator enumerator = list.GetEnumerator();
                //int counter = -1;
                //while (enumerator.MoveNext())
                //{
                //    counter++;


                //}
                return propDescriptions;
            }
            else
            {
                return base.GetProperties(context, value, attributes);
            }
        }
    }

    public class ListItemPropertyDescriptor2<T, T2> : PropertyDescriptor
    {
        private readonly IDictionary<T, T2> owner;
        private readonly T index;
        private readonly int order;

        public ListItemPropertyDescriptor2(IDictionary<T, T2> owner, T index, int order) : base("[" + index + "]", null)
        {
            this.owner = owner;
            this.index = index;
            this.order = order;
        }

        public override AttributeCollection Attributes
        {
            get
            {
                var attributes = TypeDescriptor.GetAttributes(GetValue(null), false);
                //If the Xceed expandable object attribute is not applied then apply it
                //if (!attributes.OfType<ExpandableObjectAttribute>().Any())
                //{
                //    attributes = AddAttribute(new ExpandableObjectAttribute(), attributes);
                //}

                //set the xceed order attribute
                attributes = AddAttribute(new DisplayNameAttribute(index.ToString()), attributes);
                attributes = AddAttribute(new PropertyOrderAttribute(order), attributes);
                attributes = AddAttribute(new EditorAttribute(typeof(StringArrayEditorWithPreview), typeof(StringArrayEditorWithPreview)), attributes);

                return attributes;
            }
        }
        private AttributeCollection AddAttribute(Attribute newAttribute, AttributeCollection oldAttributes)
        {
            Attribute[] newAttributes = new Attribute[oldAttributes.Count + 1];
            oldAttributes.CopyTo(newAttributes, 1);
            newAttributes[0] = newAttribute;

            return new AttributeCollection(newAttributes);
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return Value;
        }

        private T2 Value
          => owner[index];

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            owner[index] = (T2)value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType
          => owner.GetType();

        public override bool IsReadOnly
          => false;

        public override Type PropertyType
          => Value?.GetType();

    }
}
