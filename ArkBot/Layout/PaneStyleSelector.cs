using ArkBot.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArkBot.Layout
{
    public class PaneStyleSelector : StyleSelector
    {
        public Style ConsoleStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is ConsoleViewModel)
                return ConsoleStyle;

            return base.SelectStyle(item, container);
        }
    }
}
