using ArkBot.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArkBot.Modules.Application.ViewModel
{
    public sealed class ConsoleViewModel : TabViewModel
    {
        public ObservableCollection<ConsoleLogEntry> ConsoleOutput { get; set; }

        private ConsoleViewModel() : base("Console", "Console")
        {
            ConsoleOutput = new ObservableCollection<ConsoleLogEntry>();
        }

        private async Task<ConsoleViewModel> InitializeAsync()
        {
            return this;
        }

        public static Task<ConsoleViewModel> CreateAsync(bool isVisible = false)
        {
            var ret = new ConsoleViewModel { IsVisible = isVisible };
            return ret.InitializeAsync();
        }

        public void AddLog(string message, System.Windows.Media.Brush color = null)
        {
            if (message == null) return;

            System.Windows.Application.Current?.Dispatcher.Invoke(delegate
            {
                while (ConsoleOutput.Count >= 1000) ConsoleOutput.RemoveAt(0);
                ConsoleOutput.Add(new ConsoleLogEntry(message.TrimEnd('\n', '\r'), color));
            });
        }

        public void AddLogError(string message)
        {
            AddLog(message, System.Windows.Media.Brushes.Red);
        }

        public void AddLogWarning(string message)
        {
            AddLog(message, System.Windows.Media.Brushes.Orange);
        }
    }

    public class ConsoleLogEntry
    {
        public ConsoleLogEntry(string message, System.Windows.Media.Brush color = null)
        {
            When = DateTime.Now;
            Message = message;
            Color = color ?? System.Windows.Media.Brushes.Black;
        }

        public DateTime When { get; set; }
        public string Message { get; set; }
        public System.Windows.Media.Brush Color { get; set; }
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
                scrollViewer.Tag = scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight;
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
            else scrollViewer.Tag = scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight;
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
