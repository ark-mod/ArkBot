using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using ArkBot.Browser;
using ArkBot.Configuration;
using ArkBot.Extensions;
using ArkBot.Helpers;
using ArkBot.ViewModel;
using CefSharp;
using Markdig;
using Nito.AsyncEx;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace ArkBot.Controls
{
    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configuration : UserControl
    {
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(ConfigurationViewModel), typeof(Configuration), new FrameworkPropertyMetadata());

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register("SelectedObject", typeof(object), typeof(Configuration), new UIPropertyMetadata(null /*, new PropertyChangedCallback(Configuration.OnSelectedObjectChanged)*/));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(Configuration), new UIPropertyMetadata(false));

        //public static readonly RoutedEvent SelectedObjectChangedEvent = EventManager.RegisterRoutedEvent("SelectedObjectChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(Configuration));

        public ConfigurationViewModel Model
        {
            get { return GetValue(ModelProperty) as ConfigurationViewModel; }
            set { SetValue(ModelProperty, value); }
        }

        public object SelectedObject
        {
            get { return GetValue(SelectedObjectProperty); }
            set { SetValue(SelectedObjectProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private bool _propSelected = false;
        private PropertyItem _summaryBrowserSelectedProp;

        public Configuration()
        {
            //todo: must fix collection validation callback (it probably does not work for collection that are inside expandable sections)
            //todo: must make expandable sections show validation messages from ValidateExpandableAttribute where they contain fields that are invalid

            InitializeComponent();

            PART_PropertyGrid.SelectedObjectChanged += ConfigurationPropertyGrid_SelectedObjectChanged;
            PART_PropertyGrid.PropertyValueChanged += ConfigurationPropertyGrid_PropertyValueChanged;
            PART_PropertyGrid.SelectedPropertyItemChanged += ConfigurationPropertyGrid_SelectedPropertyItemChanged;
            PART_PropertyGrid.LayoutUpdated += PART_PropertyGrid_LayoutUpdated;
            PART_PropertyGrid.PropertyChanged += PART_PropertyGrid_PropertyChanged;

            SummaryBrowser.IsBrowserInitializedChanged += SummaryBrowser_IsBrowserInitializedChanged;
            var reqHandler = new RequestEventHandler();
            reqHandler.OnBeforeBrowseEvent += ReqHandler_OnBeforeBrowseEvent;
            SummaryBrowser.RequestHandler = reqHandler;

            //SelectedObject = "{Binding SelectedItem, RelativeSource={RelativeSource TemplatedParent}}"
            //IsReadOnly = "{Binding IsReadOnly, RelativeSource={RelativeSource TemplatedParent}}"

            //var myBinding = new System.Windows.Data.Binding("SelectedItem");
            //myBinding.Source = Model;
            //PART_PropertyGrid.SetBinding(PropertyGrid.SelectedObjectProperty, myBinding);
        }

        private async void PART_PropertyGrid_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("Filter"))
            {
                await UpdateValidation();
            }
        }

        //private int _selectedObjectChangedCounter = 0;
        //private int _lastLayoutUpdatedForSelectedObjectChangedCounter = 0;

        private void PART_PropertyGrid_LayoutUpdated(object sender, EventArgs e)
        {
            //var grids = PART_PropertyGrid.FindVisualChildrenWithPath<Grid>("PropertyItem/Border/Grid").ToArray();
            //if (grids == null) return;
            //foreach(var grid in grids)
            //{
            //    var binding = new Binding()
            //    {
            //        Source = grid.ColumnDefinitions[0],
            //        Path = new PropertyPath("Width"),
            //        Mode = BindingMode.OneWay,
            //    };

            //    grid.ColumnDefinitions[0].SetBinding(MaxWidthProperty, binding);
            //}

            //this does not work with sorting options
            //if (_selectedObjectChangedCounter > _lastLayoutUpdatedForSelectedObjectChangedCounter)
            //{
            //var expandSites = PART_PropertyGrid.FindVisualChildrenWithPath<Border>("Expander[expander]/Grid/Border[ExpandSite]");
            //foreach (var exp in expandSites)
            //{
            //    exp.Padding = new Thickness(exp.Padding.Left, exp.Padding.Top, 23, exp.Padding.Bottom); //add some padding on the right side to make room for validation adorners
            //}

            //var expandSites = PART_PropertyGrid.FindVisualChildrenWithPath<Border>("PropertyItem/Border");
            //foreach (var exp in expandSites)
            //{
            //    exp.Padding = new Thickness(exp.Padding.Left, exp.Padding.Top, 23, exp.Padding.Bottom); //add some padding on the right side to make room for validation adorners
            //}

            //var expandSites = PART_PropertyGrid.FindVisualChildrenWithPath<VirtualizingStackPanel>("PropertyItemsControl[PART_PropertyItemsControl]/Border/ScrollViewer/Grid/ScrollContentPresenter/ItemsPresenter/VirtualizingStackPanel");
            //foreach (var exp in expandSites)
            //{
            //    exp.Margin = new Thickness(exp.Margin.Left, exp.Margin.Top, 23, exp.Margin.Bottom); //add some margin on the right side to make room for validation adorners
            //}

            //_lastLayoutUpdatedForSelectedObjectChangedCounter = _selectedObjectChangedCounter;
            //}
        }

        //private static void OnSelectedObjectChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        //{
        //    var grid = o as PropertyGrid;
        //    if (grid != null)
        //    {
        //        //protected virtual void OnSelectedObjectChanged(object oldValue, object newValue)
        //        var mi = typeof(PropertyGrid).GetMethod("OnSelectedObjectChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        //        if (mi != null)
        //        {
        //            mi.Invoke(grid, new[] { e.OldValue, e.NewValue });
        //        }
        //        //grid.OnSelectedObjectChanged(e.OldValue, e.NewValue);
        //    }
        //}
        

        private async void ConfigurationPropertyGrid_SelectedObjectChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //_selectedObjectChangedCounter++;

            // without this recursive collection validation errors do not get set
            await UpdateValidation(trigger: true);

            _propSelected = SelectFirstProp();
        }

        private bool SelectFirstProp()
        {
            if (!SummaryBrowser.IsBrowserInitialized) return false;

            var prop = PART_PropertyGrid.Properties?.Cast<PropertyItem>().FirstOrDefault();
            if (prop == null) return false;

            PART_PropertyGrid.SelectedProperty = prop;

            return true;
        }

        private void ReqHandler_OnBeforeBrowseEvent(object sender, Browser.EventArgs.OnBeforeBrowseEventArgs e)
        {
            if (e.Request.Url.Equals("http://tmp/", StringComparison.OrdinalIgnoreCase)) return;
            else if (e.Request.Url.StartsWith("navigate://", StringComparison.OrdinalIgnoreCase))
            {
                var r = new Regex(@"^navigate://(?<to>.+)$");
                var to = r.Match(e.Request.Url)?.Groups["to"].Value;
                if (!string.IsNullOrWhiteSpace(to))
                {
                    switch (to.ToLower())
                    {
                        case "restore-default-value":
                            PART_PropertyGrid.Dispatcher.Invoke(new Action(() => {
                                var defaultAttr = _summaryBrowserSelectedProp?.PropertyDescriptor.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
                                if (defaultAttr != null) _summaryBrowserSelectedProp.Value = defaultAttr.Value;
                            }));

                            break;
                    }
                }

                e.CancelNavigation = true;
                return;
            }

            e.CancelNavigation = true;
            Process.Start(e.Request.Url);
        }

        private async void ConfigurationPropertyGrid_SelectedPropertyItemChanged(object sender, RoutedPropertyChangedEventArgs<PropertyItemBase> e)
        {
            await UpdateBrowser();
        }

        private async Task UpdateBrowser()
        {
            var prop = PART_PropertyGrid.SelectedPropertyItem as PropertyItem;
            if (SummaryBrowser.IsBrowserInitialized && prop != null)
            {
                _summaryBrowserSelectedProp = prop;
                var attr = prop.PropertyDescriptor.Attributes.OfType<ConfigurationHelpAttribute>().FirstOrDefault();
                var defaultAttr = prop.PropertyDescriptor.Attributes.OfType<DefaultValueAttribute>().FirstOrDefault();
                var remarks = attr?.Remarks != null ? Markdown.ToHtml(attr.Remarks) : null;
                var instructions = attr?.Instructions != null ? Markdown.ToHtml(attr.Instructions) : null;
                var example = attr?.Example != null ? Markdown.ToHtml(attr.Example) : null;
                var defaultValue = defaultAttr?.Value != null ? defaultAttr.Value.ToString() : null;
                var desc = prop.GetBindingExpression(PropertyItem.ValueProperty)?.DataItem as DependencyObject;
                var html = await Model.RunCompileTemplate(new ConfigurationViewModel.HelpTemplateViewModel
                {
                    displayName = prop.DisplayName,
                    description = prop.Description,
                    remarks = remarks,
                    instructions = instructions,
                    example = example,
                    defaultValue = defaultValue,
                    validationError = desc != null ? Validation.GetHasError(desc) ? Validation.GetErrors(desc)[0]?.ErrorContent.ToString() : null : null
                });

                //if (Validation.GetHasError(descriptor))
                //{
                //    var errors = Validation.GetErrors(descriptor);
                //    Validation.MarkInvalid(be, errors[0]);
                //}

                SummaryBrowser.LoadHtml(html, "http://tmp/");
            }
        }

        private void SummaryBrowser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue)
            {
                if (!_propSelected) _propSelected = SelectFirstProp();

                // Note: For these to work, make sure Address is not set in Browser Control
                //SummaryBrowser.LoadHtml("<html><head><title>Test</title></head><body><h1>Html Encoded in URL 2!</h1></body></html>", "http://tmp/");

                //SummaryBrowser.Load("about:blank");
                //SummaryBrowser.LoadString(@"<html><head><title>Test</title></head><body><h1>Html Encoded in URL 2!</h1></body></html>", "http://tmp/");
            }
        }

        /// <summary>
        /// PropertyGrid only updates validation for the PropertyItem that was changed by default. This is a way to force it to update all properties in order to support dependencies. PropertyGrid is built for .NET 4 and does not support the ValidatesOnNotifyDataErrors binding property.
        /// </summary>
        private async void ConfigurationPropertyGrid_PropertyValueChanged(object sender, Xceed.Wpf.Toolkit.PropertyGrid.PropertyValueChangedEventArgs e)
        {
            var modifiedPropertyItem = e.OriginalSource as PropertyItem;
            if (modifiedPropertyItem != null)
            {
                await UpdateValidation(modifiedPropertyItem, true);
            }
        }

        internal async Task UpdateValidation(PropertyItem modifiedPropertyItem = null, bool trigger = false)
        {
            // trigger a full revalidation
            if (trigger && PART_PropertyGrid?.SelectedObject != null)
            {
                var fi = PART_PropertyGrid?.SelectedObject.GetType().GetField("validationTemplate", BindingFlags.Instance | BindingFlags.NonPublic);
                (fi?.GetValue(PART_PropertyGrid?.SelectedObject) as ArkBot.Configuration.Validation.ValidationTemplate)?.Validate();
            }

            if (PART_PropertyGrid?.Properties == null) return;

            UpdateRecursive(modifiedPropertyItem, PART_PropertyGrid.Properties.Cast<PropertyItem>());

            await UpdateBrowser();
        }

        private void UpdateRecursive(PropertyItem modifiedPropertyItem, IEnumerable<PropertyItem> properties)
        {
            foreach (var prop in properties)
            {
                if (prop == modifiedPropertyItem) continue;

                var be = prop.GetBindingExpression(PropertyItem.ValueProperty);
                if (be != null)
                {
                    be.UpdateSource();
                    SetRedInvalidBorder(be);
                }

                if (prop.IsExpandable)
                {
                    UpdateRecursive(modifiedPropertyItem, prop.Properties.Cast<PropertyItem>());
                }
                //if (prop.Editor is CollectionControlButton && prop.Value is System.Collections.ICollection)
                //{
                //    var collection = prop.Value as System.Collections.ICollection;
                //    foreach (var item in collection)
                //    {
                //        var validationContext = new ValidationContext(item, null, null);
                //        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                //        Validator.TryValidateObject(item, validationContext, validationResults, true);
                //        var r = validationResults.FirstOrDefault();
                //        if (r != null)
                //        {
                //            Validation.MarkInvalid(be, new ValidationError(new DataErrorValidationRule(), be, r.ErrorMessage, null));
                //            break;
                //        }
                //    }
                //}
            }
        }

        internal void SetRedInvalidBorder(BindingExpression be)
        {
            if ((be != null) && IsBaseTypeName("DescriptorPropertyDefinitionBase", be.DataItem.GetType().BaseType))
            {
                var descriptor = be.DataItem as DependencyObject;
                if (Validation.GetHasError(descriptor))
                {
                    var errors = Validation.GetErrors(descriptor);
                    Validation.MarkInvalid(be, errors[0]);
                }
            }
        }

        //internal List<ValidationError> ValidationGetErrors(DependencyObject descriptor)
        //{
        //    var result = new List<ValidationError>();

        //    if (Validation.GetHasError(descriptor))
        //    {
        //        result.AddRange(Validation.GetErrors(descriptor));
        //    }

        //    if(descriptor.)

        //    return null;
        //}

        internal bool IsBaseTypeName(string name, Type t)
        {
            if (t == null) return false;
            if (t.Name.Equals(name)) return true;

            return IsBaseTypeName(name, t.BaseType);
        }
    }
    
    // Fix ScrollViewer stealing mouse wheel events preventing the PropertyGrid from scrolling in ExpandableProperties
    public static class BubbleScrollBehavior
    {
        public static readonly DependencyProperty BubbleScrollProperty =
            DependencyProperty.RegisterAttached("BubbleScroll", typeof(bool), typeof(BubbleScrollBehavior), new PropertyMetadata(false, BubbleScrollPropertyChanged));


        public static void BubbleScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var e = obj as UIElement;
            if (e != null && (bool)args.NewValue)
            {
                e.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
            }
            else
            {
                e.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
            }
        }

        private static void AssociatedObject_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer && !e.Handled)
            {
                e.Handled = true;
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        public static bool GetBubbleScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(BubbleScrollProperty);
        }

        public static void SetBubbleScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(BubbleScrollProperty, value);
        }
    }

    class IsPropertyGridConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var control = value as DependencyObject;
            if (value == null) return false;
            var result = control.FindVisualParents().Take(2).Any(x => x is PropertyItemBase);

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter
            , System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class SetGridColumnBehavior
    {
        public static readonly DependencyProperty SetGridColumnProperty =
            DependencyProperty.RegisterAttached("SetGridColumn", typeof(bool), typeof(SetGridColumnBehavior), new PropertyMetadata(false, SetGridColumnPropertyChanged));


        public static void SetGridColumnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var e = obj as Grid;
            if (e != null && (bool)args.NewValue)
            {
                var b = new Binding();
                b.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor,
                                                      typeof(PropertyGrid), 1);
                b.Path = new PropertyPath("NameColumnWidth");

                BindingOperations.SetBinding(e.ColumnDefinitions[0], ColumnDefinition.MaxWidthProperty, b);
                //e.ColumnDefinitions[0].MinWidth = 200; //not working - grip bugged and unusable after resizing
            }
        }

        public static bool GetSetGridColumn(DependencyObject obj)
        {
            return (bool)obj.GetValue(SetGridColumnProperty);
        }

        public static void SetSetGridColumn(DependencyObject obj, bool value)
        {
            obj.SetValue(SetGridColumnProperty, value);
        }
    }
}
