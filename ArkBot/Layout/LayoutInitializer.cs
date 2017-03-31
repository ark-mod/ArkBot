using ArkBot.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock.Layout;

namespace ArkBot.Layout
{
    public class LayoutInitializer : ILayoutUpdateStrategy
    {
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            var destPane = destinationContainer as LayoutAnchorablePane;
            if (destinationContainer != null && destinationContainer.FindParent<LayoutFloatingWindow>() != null)
                return false;

            if (anchorableToShow?.Content is TabViewModel)
            {
                var tabPane = layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
                if (tabPane != null)
                {
                    tabPane.Children.Add(anchorableToShow);
                    return true;
                }
            }
            else if (anchorableToShow?.Content is ToolViewModel)
            {
                var toolsPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "ToolsPane");
                if (toolsPane != null)
                {
                    toolsPane.Children.Add(anchorableToShow);
                    return true;
                }
            }

            return false;
        }


        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {

        }
    }
}
