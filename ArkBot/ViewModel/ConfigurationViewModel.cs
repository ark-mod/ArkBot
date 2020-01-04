using ArkBot.Commands;
using ArkBot.Helpers;
using ArkBot.WpfCommands;
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
    public sealed class ConfigurationViewModel : TabViewModel, IDisposable
    {
        public class HelpTemplateViewModel
        {
            public string displayName { get; set; }
            public string description { get; set; }
            public string remarks { get; set; }
            public string instructions { get; set; }
            public string example { get; set; }
            public string defaultValue { get; set; }
            public string validationError { get; set; }
        }

        public object Config { get; set; }

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

        private AsyncLazy<string> _template = new AsyncLazy<string>(async () => await FileHelper.ReadAllTextTaskAsync(new Constants().ConfigurationHelpTemplatePath));

        private ConfigurationViewModel() : base("Configuration", "Configuration")
        {
        }

        private async Task<ConfigurationViewModel> InitializeAsync()
        {
            await RunCompileTemplate(new HelpTemplateViewModel { displayName = "", description = "", instructions = "", example = "", defaultValue = null });
            return this;
        }

        public static Task<ConfigurationViewModel> CreateAsync(bool isVisible = false)
        {
            var ret = new ConfigurationViewModel { IsVisible = isVisible };
            return ret.InitializeAsync();
        }

        public async Task<string> RunCompileTemplate(HelpTemplateViewModel model)
        {
            var template = await _template;
            var html = RazorEngineService.Value.RunCompile(template, template, typeof(HelpTemplateViewModel), model);
            return html;
        }

        public ICommand SaveConfig => _saveConfig ?? (_saveConfig = new RelayCommand(parameter => OnSaveConfig(parameter), parameter => CanSaveConfig(parameter)));
        private RelayCommand _saveConfig;

        private bool CanSaveConfig(object parameter)
        {
            return true;
        }

        private void OnSaveConfig(object parameter)
        {
            //if (!File.Exists(Constants.ConfigFilePath)) return;

            // clear keyboard focus so that the configuration control triggers an update for the current field before saving
            Keyboard.ClearFocus();

            var result = MessageBox.Show("Are you sure you want to save this configuration?", "Save current configuration", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                var json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(Workspace.Constants.ConfigFilePath, json);
            }
            catch
            {
            }
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
