using ArkBot.Commands;
using ArkBot.Helpers;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Prism.Commands;
using PropertyChanged;
using RazorEngine.Configuration;
using RazorEngine.Templating;
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
    public sealed class AboutViewModel : TabViewModel, IDisposable
    {
        public class AboutTemplateViewModel
        {
            public bool hasConfig { get; set; }
            public string validationError { get; set; }
            public string configError { get; set; }
        }

        public bool HasValidConfig { get; set; }
        public string ValidationError { get; set; }
        public string ConfigError { get; set; }

        private Lazy<IRazorEngineService> _razorEngineService = new Lazy<IRazorEngineService>(() =>
        {
            var razorConfig = new TemplateServiceConfiguration
            {
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => { })
            };

            return RazorEngine.Templating.RazorEngineService.Create(razorConfig);
        });
        public Lazy<IRazorEngineService> RazorEngineService => _razorEngineService;

        private AsyncLazy<string> _template = new AsyncLazy<string>(async () => await FileHelper.ReadAllTextTaskAsync(new Constants().AboutTemplatePath));


        private AboutViewModel() : base("About", "About")
        {
        }

        private async Task<AboutViewModel> InitializeAsync()
        {
            await RunCompileTemplate(new AboutTemplateViewModel { hasConfig = true });
            return this;
        }

        public static Task<AboutViewModel> CreateAsync(bool isVisible = false)
        {
            var ret = new AboutViewModel { IsVisible = isVisible };
            return ret.InitializeAsync();
        }

        public async Task<string> RunCompileTemplate(AboutTemplateViewModel model)
        {
            var template = await _template;
            var html = RazorEngineService.Value.RunCompile(template, template, typeof(AboutTemplateViewModel), model);
            return html;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_razorEngineService != null && _razorEngineService.IsValueCreated)
                    {
                        _razorEngineService.Value.Dispose();
                        _razorEngineService = null;
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
