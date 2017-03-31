using ArkBot.Commands;
using ArkBot.Helpers;
using Newtonsoft.Json;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArkBot.ViewModel
{
    public class ConsoleViewModel : TabViewModel
    {
        public ObservableCollection<string> ConsoleOutput { get; set; }

        public ConsoleViewModel() : base("Console", "Console")
        {
            ConsoleOutput = new ObservableCollection<string>();
        }

        public void AddLog(string message)
        {
            Application.Current.Dispatcher.Invoke(delegate
            {
                ConsoleOutput.Add(message.TrimEnd('\n', '\r'));
            });
        }
    }

    public static class AutoScrollBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, AutoScrollPropertyChanged));


        public static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var scrollViewer = obj as ScrollViewer;
            if (scrollViewer != null && (bool)args.NewValue)
            {
                scrollViewer.Tag = (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight);
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                scrollViewer.ScrollToBottom();
            }
            else
            {
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null) return;

            if (e.ExtentHeightChange > 0)
            {
                if ((bool)scrollViewer.Tag) scrollViewer.ScrollToEnd();
            }
            else scrollViewer.Tag = (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight);
        }

        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }
    }
}
