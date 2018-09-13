using System.Collections.Generic;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace ArkBot.Configuration
{
    public class StringArrayEditorWithPreview : TypeEditor<PrimitiveTypeCollectionControlWithPreview>
    {
        public override FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            var editor = base.ResolveEditor(propertyItem);
            return editor;
        }

        protected override void SetControlProperties(PropertyItem propertyItem)
        {
            Editor.BorderThickness = new System.Windows.Thickness(0);
            Editor.Content = null;
        }

        protected override void SetValueDependencyProperty()
        {
            ValueProperty = PrimitiveTypeCollectionControlWithPreview.ItemsSourceProperty;
        }

        protected override void ResolveValueBinding(PropertyItem propertyItem)
        {
            var type = propertyItem.PropertyType;
            Editor.ItemsSourceType = type;

            if (type.BaseType == typeof(System.Array))
            {
                Editor.ItemType = type.GetElementType();
            }
            else if (type.BaseType.IsGenericType && (type.BaseType.GetGenericTypeDefinition() == typeof(List<>)))
            {
                var typeArguments = type.BaseType.GetGenericArguments();
                if (typeArguments.Length > 0)
                {
                    Editor.ItemType = typeArguments[0];
                }
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
}
