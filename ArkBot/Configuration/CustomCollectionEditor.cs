using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Windows.Controls.Primitives;
using System.Reflection;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;
using ArkBot.Extensions;
using ArkBot.ViewModel;
using System.Windows.Data;
using System.Collections.Concurrent;
using System.Windows.Media;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Windows.Threading;
using Nito.AsyncEx;

namespace ArkBot.Configuration
{
    public class CustomCollectionEditor : CollectionEditor
    {
        //public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(ConfigurationViewModel), typeof(CollectionControlButton), new FrameworkPropertyMetadata());

        //public ConfigurationViewModel Model
        //{
        //    get { return Editor.GetValue(ModelProperty) as ConfigurationViewModel; }
        //    set { Editor.SetValue(ModelProperty, value); }
        //}

        protected override void SetControlProperties(PropertyItem propertyItem)
        {
            RemoveRoutedEventHandlers(Editor, ButtonBase.ClickEvent);

            SetEditorContent(propertyItem);
            propertyItem.PropertyChanged += (s, e) => SetEditorContent(propertyItem);

            Editor.Foreground = Brushes.Gray;
            Editor.Padding = new Thickness(3);
            Editor.Click += new RoutedEventHandler(async (s, e) =>
            {
                var dlg = new CollectionControlDialog
                {
                    Title = "Config Editor",
                    Width = 1000,
                    Height = 800,
                    MinWidth = 800,
                    MinHeight = 500,
                    ItemsSource = Editor.ItemsSource,
                    NewItemTypes = Editor.NewItemTypes,
                    ItemsSourceType = Editor.ItemsSourceType,
                    IsReadOnly = Editor.IsReadOnly
                };

                var cc = dlg.CollectionControl;
                //var pg = cc.PropertyGrid;
                //pg.Loaded += PropertyGrid_Loaded;

                using (var viewModel = await ConfigurationViewModel.CreateAsync(false))
                {
                    //foo.SelectedObject = cc.SelectedItem;

                    //dlg.Loaded += Dlg_Loaded;

                    dlg.DataContext = viewModel;

                    if (dlg.ShowDialog() == true)
                    {
                        // trigger a full revalidation and update the grid
                        var conf = Editor.FindVisualParents().OfType<ArkBot.Controls.Configuration>().FirstOrDefault();
                        if (conf != null)
                        {
                            AsyncContext.Run(async () =>
                            {
                                await conf.UpdateValidation(trigger: true);
                            });
                        }
                    }
                }
            });
        }

        private void SetEditorContent(PropertyItem propertyItem)
        {
            var value = propertyItem.Value;
            var t = propertyItem.Value.GetType();
            var valueString = value.ToString();
            if (string.IsNullOrEmpty(valueString) || (valueString == t.UnderlyingSystemType.ToString()))
            {
                valueString = propertyItem.DisplayName;
            }

            Editor.Content = valueString;
        }

        //private DependencyPropertyListener _foo;

        //private void Dlg_Loaded(object sender, RoutedEventArgs e)
        //{
        //    var dlg = (sender as CollectionControlDialog);
        //    var cc = dlg.CollectionControl;

        //    _foo = new DependencyPropertyListener(cc, CollectionControl.SelectedItemProperty, _e =>
        //    {
        //        (dlg.DataContext as ConfigurationViewModel).SelectedObject = _e.NewValue;
        //    });
        //}

        private void PropertyGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //modify styles dynamically during runtime because it is too complicated to override the templates/styles in xaml
            var expandSites = (sender as PropertyGrid).FindVisualChildrenWithPath<Border>("Expander[expander]/Grid/Border[ExpandSite]");
            foreach (var exp in expandSites)
            {
                exp.Padding = new Thickness(exp.Padding.Left, exp.Padding.Top, 23, exp.Padding.Bottom); //add some padding on the right side to make room for validation adorners
            }
        }

        //from: https://stackoverflow.com/a/12618521
        public static void RemoveRoutedEventHandlers(UIElement elm, RoutedEvent re)
        {
            var pi = typeof(UIElement).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            var ehs = pi.GetValue(elm, null);
            if (ehs == null) return;

            var mi = ehs.GetType().GetMethod("GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var reh = (RoutedEventHandlerInfo[])mi.Invoke(ehs, new object[] { re });

            foreach (var ei in reh) elm.RemoveHandler(re, ei.Handler);
        }
    }

    //public sealed class DependencyPropertyListener : DependencyObject, IDisposable
    //{
    //    private static readonly ConcurrentDictionary<DependencyProperty, PropertyPath> Cache = new ConcurrentDictionary<DependencyProperty, PropertyPath>();

    //    private static readonly DependencyProperty ProxyProperty = DependencyProperty.Register(
    //        "Proxy",
    //        typeof(object),
    //        typeof(DependencyPropertyListener),
    //        new PropertyMetadata(null, OnSourceChanged));

    //    private readonly Action<DependencyPropertyChangedEventArgs> onChanged;
    //    private bool disposed;

    //    public DependencyPropertyListener(
    //        DependencyObject source,
    //        DependencyProperty property,
    //        Action<DependencyPropertyChangedEventArgs> onChanged = null)
    //        : this(source, Cache.GetOrAdd(property, x => new PropertyPath(x)), onChanged)
    //    {
    //    }

    //    public DependencyPropertyListener(
    //        DependencyObject source,
    //        PropertyPath property,
    //        Action<DependencyPropertyChangedEventArgs> onChanged)
    //    {
    //        this.Binding = new Binding
    //        {
    //            Source = source,
    //            Path = property,
    //            Mode = BindingMode.OneWay,
    //        };
    //        this.BindingExpression = (BindingExpression)BindingOperations.SetBinding(this, ProxyProperty, this.Binding);
    //        this.onChanged = onChanged;
    //    }

    //    public event EventHandler<DependencyPropertyChangedEventArgs> Changed;

    //    public BindingExpression BindingExpression { get; }

    //    public Binding Binding { get; }

    //    public DependencyObject Source => (DependencyObject)this.Binding.Source;

    //    public void Dispose()
    //    {
    //        if (this.disposed)
    //        {
    //            return;
    //        }

    //        this.disposed = true;
    //        BindingOperations.ClearBinding(this, ProxyProperty);
    //    }

    //    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        var listener = (DependencyPropertyListener)d;
    //        if (listener.disposed)
    //        {
    //            return;
    //        }

    //        listener.onChanged?.Invoke(e);
    //        listener.OnChanged(e);
    //    }

    //    private void OnChanged(DependencyPropertyChangedEventArgs e)
    //    {
    //        this.Changed?.Invoke(this, e);
    //    }
    //}
}
