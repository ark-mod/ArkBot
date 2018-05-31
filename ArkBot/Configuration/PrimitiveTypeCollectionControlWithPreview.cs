using ArkBot.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace ArkBot.Configuration
{
    public class PrimitiveTypeCollectionControlWithPreview : PrimitiveTypeCollectionControl
    {
        public PrimitiveTypeCollectionControlWithPreview()
        {
            Loaded += PrimitiveTypeCollectionControlWithPreview_Loaded;
        }

        private void PrimitiveTypeCollectionControlWithPreview_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var tb in this.FindVisualChildren<TextBlock>()) tb.Foreground = Brushes.Gray;
        }

        protected override void OnTextChanged(string oldValue, string newValue)
        {
            base.OnTextChanged(oldValue, newValue);
            Content = newValue?.Replace("\r\n", ", ");
        }
    }
}
