using ArkBot.Helpers;
using Nito.AsyncEx;
//using RazorEngine.Configuration;
//using RazorEngine.Templating;
using RazorLight;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ArkBot.ViewModel
{
    public sealed class AboutViewModel : TabViewModel, IDisposable
    {
        public class AboutTemplateViewModel
        {
            public bool hasConfig { get; set; }
        }

        public bool HasValidConfig { get; set; }

        //private Lazy<IRazorEngineService> _razorEngineService = new Lazy<IRazorEngineService>(() =>
        //{
        //    var razorConfig = new TemplateServiceConfiguration
        //    {
        //        DisableTempFileLocking = true,
        //        CachingProvider = new DefaultCachingProvider(t => { })
        //    };

        //    return RazorEngine.Templating.RazorEngineService.Create(razorConfig);
        //});

        private Lazy<IRazorLightEngine> _razorEngineService = new Lazy<IRazorLightEngine>(() =>
        {
            return new RazorLightEngineBuilder()
                  .UseFileSystemProject(Path.Combine(AppContext.BaseDirectory, "Resources"))
                  .UseMemoryCachingProvider()
                  .Build();
        });

        public Lazy<IRazorLightEngine> RazorEngineService => _razorEngineService;

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
            var html = await RazorEngineService.Value.CompileRenderStringAsync("about", template, model);
            //var html = RazorEngineService.Value.RunCompile(template, template, typeof(AboutTemplateViewModel), model);
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
                    //if (_razorEngineService != null && _razorEngineService.IsValueCreated)
                    //{
                    //    _razorEngineService.Value.Dispose();
                    //    _razorEngineService = null;
                    //}
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
