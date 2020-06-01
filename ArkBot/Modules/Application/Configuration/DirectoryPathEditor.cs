﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;
using Binding = System.Windows.Data.Binding;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBox = System.Windows.Controls.TextBox;

namespace ArkBot.Modules.Application.Configuration
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class DirectoryPathEditorAttribute : Attribute
    {
        public string Title { get; set; }
    }

    public class DirectoryPathEditor : ITypeEditor
    {
        private DirectoryPathEditorAttribute _attr;

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            _attr = propertyItem.PropertyDescriptor.Attributes.OfType<DirectoryPathEditorAttribute>()
                .FirstOrDefault();

            Grid panel = new Grid();
            panel.ColumnDefinitions.Add(new ColumnDefinition());
            panel.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = GridLength.Auto
            });

            TextBox textBox = new TextBox { BorderThickness = new Thickness(0), Padding = new Thickness(3) };
            textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            Binding binding = new Binding("Value"); //bind to the Value property of the PropertyItem
            binding.Source = propertyItem;
            binding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(textBox, TextBox.TextProperty, binding);

            Button button = new Button { MinWidth = 19 };
            button.Content = "...";
            button.Tag = propertyItem;
            button.Click += button_Click;
            Grid.SetColumn(button, 1);

            panel.Children.Add(textBox);
            panel.Children.Add(button);

            return panel;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            PropertyItem item = ((Button)sender).Tag as PropertyItem;
            if (null == item) return;

            var path = item.Value as string;
            if (path != null) path = Environment.ExpandEnvironmentVariables(path);

            var di = !string.IsNullOrEmpty(path) && Directory.Exists(path) ? new DirectoryInfo(path) : null;

            using (var dialog = new FolderBrowserDialog
            {
                SelectedPath = di?.FullName,
                ShowNewFolderButton = true,
                Description = string.Format(CultureInfo.CurrentCulture, _attr?.Title ?? "Select {0}", item.DisplayName)
            })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    item.Value = dialog.SelectedPath;
                }
            }
        }
    }
}
