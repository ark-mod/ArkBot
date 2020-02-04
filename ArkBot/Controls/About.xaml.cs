using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ArkBot.Browser;
using ArkBot.Configuration;
using ArkBot.Helpers;
using ArkBot.ViewModel;
using CefSharp;
using Markdig;
using Nito.AsyncEx;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace ArkBot.Controls
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(AboutViewModel), typeof(About), new FrameworkPropertyMetadata());

        public AboutViewModel Model
        {
            get { return GetValue(ModelProperty) as AboutViewModel; }
            set { SetValue(ModelProperty, value); }
        }

        public About()
        {
            InitializeComponent();

            AboutBrowser.IsBrowserInitializedChanged += AboutBrowser_IsBrowserInitializedChanged;
            var reqHandler = new RequestEventHandler();
            reqHandler.OnBeforeBrowseEvent += ReqHandler_OnBeforeBrowseEvent;
            AboutBrowser.RequestHandler = reqHandler;
        }

        private void ReqHandler_OnBeforeBrowseEvent(object sender, Browser.EventArgs.OnBeforeBrowseEventArgs e)
        {
            if (e.Request.Url.Equals("http://tmp/", StringComparison.OrdinalIgnoreCase)) return;
            else if (e.Request.Url.StartsWith("navigate://", StringComparison.OrdinalIgnoreCase))
            {
                var r = new Regex(@"^navigate://(?<to>.+)$");
                var to = r.Match(e.Request.Url)?.Groups["to"].Value;
                if(!string.IsNullOrWhiteSpace(to))
                {
                    switch(to.ToLower())
                    {
                        case "configuration":
                            Workspace.Instance.Configuration.IsActive = true;
                            break;
                    }
                }

                e.CancelNavigation = true;
                return;
            }

            e.CancelNavigation = true;
            Process.Start(e.Request.Url);
        }

        private async void AboutBrowser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue)
            {
                // Note: For these to work, make sure Address is not set in Browser Control
                string html = await Model.RunCompileTemplate(new AboutViewModel.AboutTemplateViewModel
                {
                    hasConfig = Model.HasValidConfig,
                    validationError = Model.ValidationError,
                    configError = Model.ConfigError
                });
                AboutBrowser.LoadHtml(html, "http://tmp/");
            }
        }
    }
}
