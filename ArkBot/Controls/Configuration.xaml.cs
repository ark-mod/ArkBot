using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace ArkBot.Controls
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configuration : UserControl
    {
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(Config), typeof(Configuration), new FrameworkPropertyMetadata());

        public Config Model
        {
            get { return GetValue(ModelProperty) as Config; }
            set { SetValue(ModelProperty, value); }
        }

        public Configuration()
        {
            InitializeComponent();

            ConfigurationPropertyGrid.PropertyValueChanged += ConfigurationPropertyGrid_PropertyValueChanged;
            ConfigurationPropertyGrid.PreparePropertyItem += ConfigurationPropertyGrid_PreparePropertyItem;
        }

        private void ConfigurationPropertyGrid_PreparePropertyItem(object sender, PropertyItemEventArgs e)
        {
            if (e.PropertyItem.Name.Equals("ServerExecutableArguments", StringComparison.Ordinal))
            {
                e.PropertyItem.Editor.Height = 100;
            }
        }

        /// <summary>
        /// PropertyGrid only updates validation for the PropertyItem that was changed by default. This is a way to force it to update all properties in order to support dependencies. PropertyGrid is built for .NET 4 and does not support the ValidatesOnNotifyDataErrors binding property.
        /// </summary>
        private void ConfigurationPropertyGrid_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            var modifiedPropertyItem = e.OriginalSource as PropertyItem;
            if (modifiedPropertyItem != null)
            {
                var mi = typeof(PropertyItem).GetMethod("SetRedInvalidBorder",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (mi == null) return;

                var grid = sender as PropertyGrid;
                UpdateRecursive(mi, modifiedPropertyItem, grid.Properties.Cast<PropertyItem>());
            }
        }

        private void UpdateRecursive(MethodInfo miSetRedInvalidBorder, PropertyItem modifiedPropertyItem, IEnumerable<PropertyItem> properties)
        {
            foreach (var prop in properties)
            {
                if (prop == modifiedPropertyItem) continue;

                var be = prop.GetBindingExpression(PropertyItem.ValueProperty);
                if (be != null)
                {
                    be.UpdateSource();
                    miSetRedInvalidBorder.Invoke(prop, new[] { be });
                }

                if (prop.IsExpandable)
                {
                    UpdateRecursive(miSetRedInvalidBorder, modifiedPropertyItem, prop.Properties.Cast<PropertyItem>());
                }
            }
        }
    }
}
