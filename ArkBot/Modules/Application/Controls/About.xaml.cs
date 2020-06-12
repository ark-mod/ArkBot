using ArkBot.Modules.Application.Browser;
using ArkBot.Modules.Application.Browser.EventArgs;
using ArkBot.Modules.Application.ViewModel;
using CefSharp;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ArkBot.Modules.Application.Controls
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

        private void ReqHandler_OnBeforeBrowseEvent(object sender, OnBeforeBrowseEventArgs e)
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
                        case "configuration":
                            Workspace.Instance.Configuration.IsActive = true;
                            break;
                    }
                }

                e.CancelNavigation = true;
                return;
            }

            e.CancelNavigation = true;

            Process.Start(new ProcessStartInfo(e.Request.Url)
            {
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private async void AboutBrowser_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
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
