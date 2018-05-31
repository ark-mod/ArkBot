using ArkBot.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace ArkBot.Layout
{
    public class PaneTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ConsoleTemplate { get; set; }
        public DataTemplate ConfigurationTemplate { get; set; }
        public DataTemplate AboutTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is ConsoleViewModel)
                return ConsoleTemplate;
            if (item is ConfigurationViewModel)
                return ConfigurationTemplate;
            if (item is AboutViewModel)
                return AboutTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}
