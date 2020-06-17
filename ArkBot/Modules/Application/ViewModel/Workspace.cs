using ArkBot.Ark;
using ArkBot.Helpers;
using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.Application.Data;
using ArkBot.Modules.Application.Data.ExternalImports;
using ArkBot.Modules.Application.Services;
using ArkBot.Modules.AuctionHouse;
using ArkBot.Modules.Database;
using ArkBot.Modules.Discord;
using ArkBot.Modules.Prometheus;
using ArkBot.Modules.Shared;
using ArkBot.Modules.WebApp;
using ArkBot.Utils;
using ArkBot.Utils.Helpers;
using ArkBot.Utils.WPF;
using ArkSavegameToolkitNet;
using ArkSavegameToolkitNet.Domain;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Integration.SignalR;
using Certes;
using Certes.Acme;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using log4net.Config;
using Markdig;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nito.AsyncEx;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ArkBot.Modules.Application.ViewModel
{
    public sealed class Workspace : ViewModelBase, IDisposable
    {
        public struct Constants
        {
            public const string ConfigFilePath = @"config.json";
            public const string ArkConfigFilePath = @"ark.json";
            public const string DefaultConfigFilePath = @"defaultconfig.json";
            public const string LayoutFilePath = @".\Layout.config";

            public const string AppId = "bb61c4d0-0060-401b-bf8f-4747cbd86d9d"; // this is the package tag from the project file
        }

        public static Workspace Instance => _instance;
        private static Workspace _instance;

        public static AsyncLazy<Workspace> AsyncInstance = new AsyncLazy<Workspace>(async () =>
        {
            return _instance ?? await CreateAsync();
        });

        public ObservableCollection<PaneViewModel> Panes { get; private set; }

        public ConsoleViewModel Console { get; private set; }

        public ConfigurationViewModel Configuration { get; private set; }

        public AboutViewModel About { get; private set; }


        public DelegateCommand<System.ComponentModel.CancelEventArgs> ClosingCommand { get; private set; }
        public DelegateCommand ShowOrHideCommand { get; private set; }
        public DelegateCommand TaskIconDoubleClickCommand { get; private set; }

        public ICommand ExitCommand => _exitCommand ?? (_exitCommand = new RelayCommand(parameter => OnExit(parameter), parameter => CanExit(parameter)));
        private RelayCommand _exitCommand;

        public ICommand OpenWebAppCommand => _openWebAppCommand ?? (_openWebAppCommand = new RelayCommand(parameter => OnOpenWebApp(parameter), parameter => CanOpenWebApp(parameter)));
        private RelayCommand _openWebAppCommand;

        public ICommand ReloadPartialConfig => _reloadPartialConfig ?? (_reloadPartialConfig = new RelayCommand(parameter => OnReloadPartialConfig(parameter), parameter => CanReloadPartialConfig(parameter)));
        private RelayCommand _reloadPartialConfig;

        public ObservableCollection<MenuItemViewModel> ManuallyUpdateServers { get; set; }
        public ObservableCollection<MenuItemViewModel> ManuallyUpdateClusters { get; set; }

        internal static IContainer Container { get; set; }

        public bool SkipExtractNextRestart { get; set; }

        private SavedState _savedstate = null;
        private IDisposable _webapp;

        private ArkContextManager _contextManager;
        internal IConfig _config;

        internal bool _isUIHidden = false;
        internal bool _startedWithoutErrors = false;

        private Workspace()
        {
            //do not create viewmodels or load data here, or avalondock layout deserialization will fail
            Panes = new ObservableCollection<PaneViewModel>();
            ManuallyUpdateServers = new ObservableCollection<MenuItemViewModel>();
            ManuallyUpdateClusters = new ObservableCollection<MenuItemViewModel>();
            ClosingCommand = new DelegateCommand<System.ComponentModel.CancelEventArgs>(OnClosing);

            PropertyChanged += Workspace_PropertyChanged;

            //if markdig is not used it will not be loaded before being used by razor template (hack)
            var tmp = Markdown.ToHtml(@"**hack**");
        }

        private async Task<Workspace> InitializeAsync()
        {
            Console = await ConsoleViewModel.CreateAsync(true);
            Configuration = await ConfigurationViewModel.CreateAsync(true);
            About = await AboutViewModel.CreateAsync(true);
            Panes.AddRange(new PaneViewModel[] { About, Console, Configuration });

            await Init();

            return this;
        }

        private async static Task<Workspace> CreateAsync()
        {
            _instance = new Workspace();
            return await _instance.InitializeAsync();
        }

        private void Workspace_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SkipExtractNextRestart) && _savedstate != null)
            {
                _savedstate.SkipExtractNextRestart = SkipExtractNextRestart;
                _savedstate.Save();
            }
        }

        private void OnManuallyTriggerServerUpdate(string serverKey)
        {
            var context = _contextManager.GetServer(serverKey);
            if (context == null) MessageBox.Show($"Could not find server instance '{serverKey}'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            _contextManager.QueueUpdateServerManual(context);
        }

        private void OnManuallyTriggerClusterUpdate(string clusterKey)
        {
            var context = _contextManager.GetCluster(clusterKey);
            if (context == null) MessageBox.Show($"Could not find cluster instance '{clusterKey}'", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            _contextManager.QueueUpdateClusterManual(context);
        }

        private void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_runDiscordBotTask != null)
            {
                _runDiscordBotCts?.Cancel();
                if (_runDiscordBotTask.Status == TaskStatus.Running) Task.WaitAny(_runDiscordBotTask);
            }
        }

        private bool CanExit(object parameter)
        {
            return true;
        }

        private void OnExit(object parameter)
        {
            System.Windows.Application.Current.MainWindow?.Close();
        }

        private bool CanOpenWebApp(object parameter)
        {
            return !string.IsNullOrEmpty(_config?.WebApp?.ExternalUrl);
        }

        private void OnOpenWebApp(object parameter)
        {
            Process.Start(new ProcessStartInfo(_config.WebApp.ExternalUrl)
            {
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private bool CanReloadPartialConfig(object parameter)
        {
            return true;
        }

        private void OnReloadPartialConfig(object parameter)
        {
            if (!File.Exists(Constants.ConfigFilePath)) return;

            var config = (Config)null;
            string exceptionMessage = null;
            try
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Constants.ConfigFilePath));
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
            }
            if (_config == null)
            {
                MessageBox.Show("Config.json is not a valid configuration file." + (!string.IsNullOrWhiteSpace(exceptionMessage) ? "\n" + exceptionMessage : ""), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //reload partial configuration
            _config.WebApp.AccessControl = config.WebApp.AccessControl;
            _config.WebApp.UserRoles = config.WebApp.UserRoles;
            _config.Discord.AccessControl = config.Discord.AccessControl;
        }

        static void WriteAndWaitForKey(params string[] msgs)
        {
            foreach (var msg in msgs) if (msg != null) System.Console.WriteLine(msg);
        }

        internal async Task Init()
        {
            System.Console.WriteLine("ARK Bot");
            System.Console.WriteLine("------------------------------------------------------");
            System.Console.WriteLine();

            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("App.config")); //"log4net.config"

            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                WriteAndWaitForKey($@"This application must be run as administrator in order to function properly.");
                return;
            }

            //ArkToolkitDomain.Initialize();
            //File.WriteAllText(@"ark.json", JsonConvert.SerializeObject(ArkSavegameToolkitNet.ArkToolkitSettings.Instance, Formatting.Indented));

            var arkConfigCustom = false;
            if (File.Exists(Constants.ArkConfigFilePath))
            {
                try
                {
                    // use custom settings from ark.json
                    ArkToolkitSettings.Instance.Setup(JsonConvert.DeserializeObject<ArkToolkitSettings>(File.ReadAllText(Constants.ArkConfigFilePath)));
                    arkConfigCustom = true;
                }
                catch (Exception ex)
                {
                    Console.AddLog($@"Error loading 'ark.config'. Using default config. (""{ex.Message}"")");
                    Logging.LogException("Error loading 'ark.config'. Using default config.", ex, GetType());
                }
            }

            if (!arkConfigCustom)
            {
                // initialize default settings
                ArkToolkitDomain.Initialize();
            }

            _config = null;
            string validationMessage = null;
            string errorMessage = null;
            if (File.Exists(Constants.ConfigFilePath))
            {
                try
                {
                    _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Constants.ConfigFilePath));
                    if (_config != null)
                    {
                        if (_config.Discord == null) _config.Discord = new DiscordConfigSection();
                        _config.SetupDefaults();
                    }
                    else errorMessage = "Config.json is empty. Please delete it and restart the application.";
                }
                catch (Exception ex)
                {
                    validationMessage = ex.Message;
                }
            }
            var hasValidConfig = _config != null;
            if (_config == null)
            {
                //load defaultconfig
                if (!File.Exists(Constants.DefaultConfigFilePath))
                {
                    WriteAndWaitForKey($@"The required file defaultconfig.json is missing from application directory. Please redownload the application.");
                    return;
                }

                try
                {
                    _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Constants.DefaultConfigFilePath));
                }
                catch (Exception ex)
                {
                }

                WriteAndWaitForKey(
                    $@"The file config.json is empty or contains errors. Skipping automatic startup...",
                    validationMessage);
            }

            Configuration.Config = _config as Config;
            About.HasValidConfig = hasValidConfig;
            About.ValidationError = validationMessage;
            About.ConfigError = errorMessage;

            if (!hasValidConfig)
            {
                About.IsActive = true;
                return;
            }
            else
            {
                About.IsVisible = false;
                Console.IsActive = true;
            }

            string errors = ValidateConfig();
            if (errors.Length > 0)
            {
                WriteAndWaitForKey(errors);
                return;
            }

            IProgress<string> progress = new Progress<string>(message =>
            {
                Console.AddLog(message);
            });

            var constants = new Shared.Constants();

            //if (config.Debug)
            //{
            //    //we reset the state so that every run will be the same
            //    if (File.Exists(constants.DatabaseFilePath)) File.Delete(constants.DatabaseFilePath);
            //    if (File.Exists(constants.SavedStateFilePath)) File.Delete(constants.SavedStateFilePath);

            //    //optionally use a saved database state
            //    var databaseStateFilePath = Path.Combine(config.JsonOutputDirPath, "Database.state");
            //    if (File.Exists(databaseStateFilePath)) File.Copy(databaseStateFilePath, constants.DatabaseFilePath);
            //}

            _savedstate = null;
            try
            {
                if (File.Exists(constants.SavedStateFilePath))
                {
                    _savedstate = JsonConvert.DeserializeObject<SavedState>(File.ReadAllText(constants.SavedStateFilePath));
                    _savedstate._Path = constants.SavedStateFilePath;
                }
            }
            catch { /*ignore exceptions */}
            _savedstate = _savedstate ?? new SavedState(constants.SavedStateFilePath);
            
            var discord = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = _config.Discord.LogLevel
            });
            discord.Log += msg =>
            {
                Console.AddLog(msg.Message);
                return Task.CompletedTask;
            };

            var discordCommands = new CommandService(new CommandServiceConfig
            {
            });

            var anonymizeData = new ArkBotAnonymizeData();

            //setup dependency injection
            var thisAssembly = Assembly.GetExecutingAssembly();
            var builder = new ContainerBuilder();

            builder.RegisterType<ArkServerContext>().AsSelf();
            builder.RegisterInstance(anonymizeData).AsSelf().As<ArkAnonymizeData>();
            if (_config.UseCompatibilityChangeWatcher) builder.RegisterType<ArkSaveFileWatcherTimer>().As<IArkSaveFileWatcher>();
            else builder.RegisterType<ArkSaveFileWatcher>().As<IArkSaveFileWatcher>();
            builder.RegisterInstance(discord).AsSelf();
            builder.RegisterInstance(discordCommands).AsSelf();
            builder.RegisterType<AutofacDiscordServiceProvider>().As<IServiceProvider>().SingleInstance();
            builder.RegisterType<ArkDiscordBot>();
            builder.RegisterInstance(constants).As<IConstants>();
            builder.RegisterInstance(_savedstate).As<ISavedState>();
            builder.RegisterInstance(_config as Config).As<IConfig>();
            builder.RegisterType<EfDatabaseContext>().AsSelf().As<IEfDatabaseContext>()
                .WithParameter(new TypedParameter(typeof(string), _config.DatabaseConnectionString));
            builder.RegisterType<EfDatabaseContextFactory>();
            builder.RegisterType<DatabaseRepo>().As<IDatabaseRepo>().SingleInstance();

            builder.RegisterType<ArkServerService>().As<IArkServerService>().SingleInstance();
            builder.RegisterType<SavegameBackupService>().As<ISavegameBackupService>().SingleInstance();
            builder.RegisterType<PlayerLastActiveService>().As<IPlayerLastActiveService>().SingleInstance();
            builder.RegisterType<LogCleanupService>().As<ILogCleanupService>().SingleInstance();

            builder.RegisterHubs(Assembly.GetExecutingAssembly());

            builder.RegisterType<ArkContextManager>().WithParameter(new TypedParameter(typeof(IProgress<string>), progress)).AsSelf().SingleInstance();
            builder.RegisterType<DiscordManager>().AsSelf().SingleInstance();
            builder.RegisterType<ScheduledTasksManager>().AsSelf().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies).SingleInstance();

            builder.RegisterType<NotificationManager>().AsSelf().SingleInstance();
            builder.RegisterType<PrometheusManager>().AsSelf().SingleInstance();
            builder.RegisterType<AuctionHouseManager>().AsSelf().SingleInstance();

            Container = builder.Build();

            //update database
            try
            {
                using (var db = Container.Resolve<EfDatabaseContext>())
                {
                    db.Database.Migrate();
                }
            }
            catch (SqlException ex)
            {
                Console.AddLog($@"Error initializing Microsoft SQL Server (""{ex.Message}"")");
                Logging.LogException("Error initializing Microsoft SQL Server.", ex, GetType());
                return;
            }

            _contextManager = Container.Resolve<ArkContextManager>();
            //server/cluster contexts
            if (_config.Clusters?.Count > 0)
            {
                foreach (var cluster in _config.Clusters)
                {
                    var context = new ArkClusterContext(cluster, anonymizeData);
                    _contextManager.AddCluster(context);
                }
            }

            if (_config.Servers?.Count > 0)
            {
                var playerLastActiveService = Container.Resolve<IPlayerLastActiveService>();
                var backupService = Container.Resolve<ISavegameBackupService>();
                var logCleanupService = Container.Resolve<ILogCleanupService>();
                foreach (var server in _config.Servers)
                {
                    var clusterContext = _contextManager.GetCluster(server.ClusterKey);
                    var context = Container.Resolve<ArkServerContext>(
                        new TypedParameter(typeof(ServerConfigSection), server),
                        new TypedParameter(typeof(ArkClusterContext), clusterContext));
                    var initTask = context.Initialize(); //fire and forget
                    _contextManager.AddServer(context);
                }

                // Initialize managers so that they are ready to handle events such as ArkContextManager.InitializationCompleted-event.
                var scheduledTasksManager = Container.Resolve<ScheduledTasksManager>();
                var notificationManager = Container.Resolve<NotificationManager>();
                // <GHOST DIVISION>
                var prometheusManager = Container.Resolve<PrometheusManager>();
                var auctionHouseManager = Container.Resolve<AuctionHouseManager>();
                // </GHOST DIVISION>

                // Trigger manual updates for all servers (initialization)
                foreach (var context in _contextManager.Servers)
                {
                    ManuallyUpdateServers.Add(new MenuItemViewModel
                    {
                        Header = context.Config.Key,
                        Command = new DelegateCommand<string>(OnManuallyTriggerServerUpdate),
                        CommandParameter = context.Config.Key
                    });
                    _contextManager.QueueUpdateServerManual(context);
                }

                // Trigger manual updates for all clusters (initialization)
                foreach (var context in _contextManager.Clusters)
                {
                    ManuallyUpdateClusters.Add(new MenuItemViewModel
                    {
                        Header = context.Config.Key,
                        Command = new DelegateCommand<string>(OnManuallyTriggerClusterUpdate),
                        CommandParameter = context.Config.Key
                    });
                    _contextManager.QueueUpdateClusterManual(context);
                }

                // <GHOST DIVISION>
                // run prometheus endpoint
                if (_config.Prometheus.Enabled)
                {
                    prometheusManager.Start();
                }
                else Console.AddLog("Prometheus is disabled.");

                // run auction house manager
                if (_config.AuctionHouse.Enabled)
                {
                    auctionHouseManager.Start();
                }
                else Console.AddLog("Auction House monitor is disabled.");
                // </GHOST DIVISION>
            }

            //run the discord bot
            if (_config.Discord.Enabled)
            {
                _runDiscordBotCts = new CancellationTokenSource();
                _runDiscordBotTask = await Task.Factory.StartNew(async () => await RunDiscordBot(), _runDiscordBotCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            else Console.AddLog("Discord bot is disabled.");

            //load the server multipliers data
            await ArkServerMultipliers.Instance.LoadOrUpdate();

            var modIds = _config.Servers?.SelectMany(x => x.ModIds).Distinct().ToArray() ?? new int[] { };

            //load the species stats data
            await ArkSpeciesStats.Instance.LoadOrUpdate(modIds);

            //load the items data
            await ArkItems.Instance.LoadOrUpdate(modIds);

            //ssl
            if (_config.WebApp.Ssl?.Enabled == true)
            {
                var path = $"{_config.WebApp.Ssl.Name}.pfx";
                var revoke = false;
                var renew = false;
                if (File.Exists(path))
                {
                    try
                    {
                        using (var rlt = new X509Certificate2(path, _config.WebApp.Ssl.Password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet))
                        {
                            if (DateTime.Now < rlt.NotBefore || DateTime.Now > rlt.NotAfter.AddDays(-31))
                            {
                                using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                                {
                                    store.Open(OpenFlags.ReadWrite);
                                    if (store.Certificates.Contains(rlt)) store.Remove(rlt);
                                    store.Close();
                                }

                                renew = revoke = true;
                            }
                        }
                    }
                    catch (Exception ex) { Logging.LogException("Failed to remove ssl certificate from store.", ex, GetType()); }
                }
                else renew = true;

                if (renew)
                {
                    var success = false;
                    Console.AddLog(@"SSL Certificate request issued...");
                    AcmeContext acme = null;
                    try
                    {
                        var pathAccountKey = $"{_config.WebApp.Ssl.Name}-account.pem";
                        if (File.Exists(pathAccountKey))
                        {
                            var accountKey = KeyFactory.FromPem(File.ReadAllText(pathAccountKey));

                            acme = new AcmeContext(WellKnownServers.LetsEncryptV2, accountKey);
                            var account = await acme.Account();
                        }
                        else
                        {
                            acme = new AcmeContext(WellKnownServers.LetsEncryptV2);
                            var account = await acme.NewAccount(_config.WebApp.Ssl.Email, true);

                            var pemKey = acme.AccountKey.ToPem();
                            File.WriteAllText(pathAccountKey, pemKey);
                        }

                        var order = await acme.NewOrder(_config.WebApp.Ssl.Domains);

                        var authz = (await order.Authorizations()).First();
                        var httpChallenge = await authz.Http();
                        var keyAuthz = httpChallenge.KeyAuthz;

                        using (var webapp = Host.CreateDefaultBuilder()
                            .UseServiceProviderFactory(new AutofacChildLifetimeScopeServiceProviderFactory(Container.BeginLifetimeScope("AspNetCore_IsolatedRoot_SSL_Renew")))
                            .ConfigureWebHostDefaults(webBuilder =>
                            {
                                webBuilder
                                    .ConfigureLogging(logging =>
                                    {
                                        logging.ClearProviders();
                                        logging.AddDebug();
                                        logging.AddEventLog();
                                        logging.AddEventSourceLogger();
                                        //todo: add log4net logging
                                        //possible to catch exceptions and serialize detailed exception logs?
                                    })
                                    .UseUrls(_config.WebApp.Ssl.ChallengeListenPrefix)
                                    .Configure((appBuilder) =>
                                    {
                                        appBuilder.UseRouting();
                                        appBuilder.UseEndpoints(endpoints =>
                                        {
                                            endpoints.MapGet($"/.well-known/acme-challenge/{httpChallenge.Token}", context =>
                                            {
                                                context.Response.ContentType = "application/text";
                                                context.Response.StatusCode = StatusCodes.Status200OK;
                                                return context.Response.WriteAsync(keyAuthz);
                                            });
                                        });
                                    });
                            }).Build())
                        {
                            (webapp as IHost).Start();

                            var result = await httpChallenge.Validate();
                            while (result.Status == Certes.Acme.Resource.ChallengeStatus.Pending || result.Status == Certes.Acme.Resource.ChallengeStatus.Processing)
                            {
                                await Task.Delay(500);
                                result = await httpChallenge.Resource();
                            }

                            if (result.Status == Certes.Acme.Resource.ChallengeStatus.Valid)
                            {
                                var privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);
                                var cert = await order.Generate(new CsrInfo
                                {
                                    CountryName = new RegionInfo(CultureInfo.CurrentCulture.Name).TwoLetterISORegionName,
                                    Locality = "Web",
                                    Organization = "ARK Bot",
                                }, privateKey);

                                if (revoke)
                                {
                                    //await acme.RevokeCertificate(
                                    //    File.ReadAllBytes(path), 
                                    //    Certes.Acme.Resource.RevocationReason.Superseded, 
                                    //    KeyFactory.FromPem(File.ReadAllText(pathPrivateKey)));

                                    if (File.Exists(path)) File.Delete(path);
                                }

                                //File.WriteAllText(pathPrivateKey, privateKey.ToPem());
                                var pfxBuilder = cert.ToPfx(privateKey);
                                var pfx = pfxBuilder.Build(_config.WebApp.Ssl.Name, _config.WebApp.Ssl.Password);
                                File.WriteAllBytes(path, pfx);

                                success = true;
                            }
                        }

                        if (success) Console.AddLog(@"SSL Certificate request completed!");
                        else Console.AddLog(@"SSL Certificate challenge failed!");
                    }
                    catch (Exception ex)
                    {
                        Console.AddLog($@"SSL Certificate request failed! (""{ex.Message}"")");
                        Logging.LogException("Failed to issue ssl certificate.", ex, GetType());
                    }
                }

                // clean up all bindings created by ark bot
                {
                    var (success, output) = await ProcessHelper.RunCommandLine("netsh http show sslcert", _config);
                    if (success && !string.IsNullOrEmpty(output))
                    {
                        var r = new Regex($@"(?:(?:\s*IP\:port\s*\:\s*(?<ip>[^\:]+?)\:(?<port>\d+))|(?:\s*Hostname\:port\s*\:\s*(?<hostname>[^\:]+?)\:(?<port>\d+))).*Application ID\s*\:\s*{{{Constants.AppId}}}", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        var sections = output.Split("\r\n\r\n\r\n", StringSplitOptions.RemoveEmptyEntries);
                        foreach (var section in sections)
                        {
                            var m = r.Match(section);
                            if (!m.Success) continue;

                            if (m.Groups["hostname"].Success && m.Groups["port"].Success)
                            {
                                await ProcessHelper.RunCommandLine($"netsh http delete sslcert hostnameport={m.Groups["hostname"].Value}:{m.Groups["port"].Value}", _config);
                            }
                            else if (m.Groups["ip"].Success && m.Groups["port"].Success)
                            {
                                await ProcessHelper.RunCommandLine($"netsh http delete sslcert ipport={m.Groups["ip"].Value}:{m.Groups["port"].Value}", _config);
                            }
                        }
                    }
                }
            }

            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-3.1
            _webapp = Host.CreateDefaultBuilder()
                 .UseServiceProviderFactory(new AutofacChildLifetimeScopeServiceProviderFactory(Container.BeginLifetimeScope("AspNetCore_IsolatedRoot")))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                        {
                            options.Listen(IPEndPoint.Parse(_config.WebApp.IPEndpoint), listenOptions =>
                            {
                                if (_config.WebApp.Ssl.Enabled) listenOptions.UseHttps($"../{_config.WebApp.Ssl.Name}.pfx", _config.WebApp.Ssl.Password);
                            });
                        })
                        .ConfigureLogging(logging =>
                        {
                            logging.ClearProviders();
                            logging.AddDebug();
                            logging.AddEventLog();
                            logging.AddEventSourceLogger();
                        })
                        .UseContentRoot(Path.Combine(AppContext.BaseDirectory, "WebApp")) //Directory.GetCurrentDirectory()
                        .UseWebRoot(Path.Combine(AppContext.BaseDirectory, "WebApp"))
                        .UseStartup<WebAppStartup>();
                }).Build();
            (_webapp as IHost).Start();
            Console.AddLog("Web App started");

            //Indicates the application started without errors, required for auto hide on startup
            _startedWithoutErrors = true;

        }

        private string ValidateConfig()
        {
            var sb = new StringBuilder();

            // attempt to create temp directory if it does not exist
            if (!string.IsNullOrWhiteSpace(_config.TempFileOutputDirPath) && !Directory.Exists(Environment.ExpandEnvironmentVariables(_config.TempFileOutputDirPath)))
            {
                try
                {
                    Directory.CreateDirectory(Environment.ExpandEnvironmentVariables(_config.TempFileOutputDirPath));
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(_config.TempFileOutputDirPath) || !Directory.Exists(Environment.ExpandEnvironmentVariables(_config.TempFileOutputDirPath)))
            {
                sb.AppendLine($@"Error: {nameof(_config.TempFileOutputDirPath)} is not a valid directory path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.TempFileOutputDirPath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.SteamApiKey))
            {
                sb.AppendLine($@"Error: {nameof(_config.SteamApiKey)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.SteamApiKey))}");
                sb.AppendLine();
            }
            if (_config.Backups.Enabled && (string.IsNullOrWhiteSpace(_config.Backups.BackupsDirectoryPath) || !FileHelper.IsValidDirectoryPath(_config.Backups.BackupsDirectoryPath)))
            {
                sb.AppendLine($@"Error: {nameof(_config.Backups.BackupsDirectoryPath)} is not a valid directory path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.Backups.BackupsDirectoryPath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.WebApp.IPEndpoint))
            {
                sb.AppendLine($@"Error: {nameof(_config.WebApp.IPEndpoint)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.WebApp.IPEndpoint))}");
                sb.AppendLine();
            }
            if (_config.WebApp?.Ssl?.Enabled == true)
            {
                if (string.IsNullOrWhiteSpace(_config.WebApp.Ssl.Name))
                {
                    sb.AppendLine($@"Error: {nameof(_config.WebApp.Ssl)}.{nameof(_config.WebApp.Ssl.Name)} is not set.");
                    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config.WebApp.Ssl, nameof(_config.WebApp.Ssl.Name))}");
                    sb.AppendLine();
                }
                if (string.IsNullOrWhiteSpace(_config.WebApp.Ssl.Password))
                {
                    sb.AppendLine($@"Error: {nameof(_config.WebApp.Ssl)}.{nameof(_config.WebApp.Ssl.Password)} is not set.");
                    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config.WebApp.Ssl, nameof(_config.WebApp.Ssl.Password))}");
                    sb.AppendLine();
                }
                if (string.IsNullOrWhiteSpace(_config.WebApp.Ssl.Email))
                {
                    sb.AppendLine($@"Error: {nameof(_config.WebApp.Ssl)}.{nameof(_config.WebApp.Ssl.Email)} is not set.");
                    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config.WebApp.Ssl, nameof(_config.WebApp.Ssl.Email))}");
                    sb.AppendLine();
                }
                if (!(_config.WebApp.Ssl.Domains?.Count >= 1) || _config.WebApp.Ssl.Domains.Any(x => string.IsNullOrWhiteSpace(x)))
                {
                    sb.AppendLine($@"Error: {nameof(_config.WebApp.Ssl)}.{nameof(_config.WebApp.Ssl.Domains)} is not set.");
                    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config.WebApp.Ssl, nameof(_config.WebApp.Ssl.Domains))}");
                    sb.AppendLine();
                }
                if (string.IsNullOrWhiteSpace(_config.WebApp.Ssl.ChallengeListenPrefix))
                {
                    sb.AppendLine($@"Error: {nameof(_config.WebApp.Ssl)}.{nameof(_config.WebApp.Ssl.ChallengeListenPrefix)} is not set.");
                    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config.WebApp.Ssl, nameof(_config.WebApp.Ssl.ChallengeListenPrefix))}");
                    sb.AppendLine();
                }
            }
            if (_config.Discord.Enabled)
            {
                if (string.IsNullOrWhiteSpace(_config.Discord.BotToken))
                {
                    sb.AppendLine($@"Error: {nameof(_config.Discord)}.{nameof(_config.Discord.BotToken)} is not set.");
                    sb.AppendLine($@"Expected value: {
                            ValidationHelper.GetDescriptionForMember(_config.Discord, nameof(_config.Discord.BotToken))
                        }");
                    sb.AppendLine();
                }
                if (string.IsNullOrWhiteSpace(_config.Discord.MemberRoleName)) _config.Discord.MemberRoleName = "ark";
            }

            var clusterkeys = _config.Clusters?.Select(x => x.Key).ToArray();
            var serverkeys = _config.Servers?.Select(x => x.Key).ToArray();
            if (serverkeys?.Length > 0 && serverkeys.Length != serverkeys.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                sb.AppendLine($@"Error: {nameof(_config.Servers)} contain non-unique keys.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.Servers))}");
                sb.AppendLine();
            }
            if (clusterkeys?.Length > 0 && clusterkeys.Length != clusterkeys.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                sb.AppendLine($@"Error: {nameof(_config.Clusters)} contain non-unique keys.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.Clusters))}");
                sb.AppendLine();
            }
            if (_config.Servers?.Count > 0)
            {
                foreach (var server in _config.Servers)
                {
                    if (server.ClusterKey != null && !clusterkeys.Contains(server.ClusterKey))
                    {
                        sb.AppendLine($@"Error: {nameof(_config.Servers)}.{nameof(server.ClusterKey)} reference missing cluster key ""{server.ClusterKey}"".");
                        sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(server, nameof(server.ClusterKey))}");
                        sb.AppendLine();
                    }
                    if (string.IsNullOrWhiteSpace(server.SaveFilePath) || !File.Exists(server.SaveFilePath))
                    {
                        sb.AppendLine($@"Error: {nameof(_config.Servers)}.{nameof(server.SaveFilePath)} is not a valid file path for server instance ""{server.Key}"".");
                        sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(server, nameof(server.SaveFilePath))}");
                        sb.AppendLine();
                    }
                    if (string.IsNullOrWhiteSpace(server.Ip))
                    {
                        sb.AppendLine($@"Error: {nameof(_config.Servers)}.{nameof(server.Ip)} is not set for server instance ""{server.Key}"".");
                        sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(server, nameof(server.Ip))}");
                        sb.AppendLine();
                    }
                    if (server.QueryPort <= 0)
                    {
                        sb.AppendLine($@"Error: {nameof(_config.Servers)}.{nameof(server.QueryPort)} is not valid for server instance ""{server.Key}"".");
                        sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(server, nameof(server.QueryPort))}");
                        sb.AppendLine();
                    }
                }
            }
            if (_config.Clusters?.Count > 0)
            {
                foreach (var cluster in _config.Clusters)
                {
                    if (string.IsNullOrWhiteSpace(cluster.SavePath) || !Directory.Exists(cluster.SavePath))
                    {
                        sb.AppendLine($@"Error: {nameof(_config.Servers)}.{nameof(cluster.SavePath)} is not a valid directory path for cluster instance ""{cluster.Key}"".");
                        sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(cluster, nameof(cluster.SavePath))}");
                        sb.AppendLine();
                    }
                }
            }

            if (_config.AnonymizeWebApiData)
            {
                System.Console.WriteLine("Anonymizing all data in the WebAPI (anonymizeWebApiData=true)" + Environment.NewLine);
            }

            //load aliases and check integrity
            var aliases = ArkSpeciesAliases.Instance;
            if (aliases == null || !aliases.CheckIntegrity)
            {
                sb.AppendLine($@"Error: ""{ArkSpeciesAliases._filepath}"" is missing, contains invalid json or duplicate aliases.");
                if (aliases != null)
                {
                    foreach (var duplicateAlias in aliases.Aliases?.SelectMany(x => x).GroupBy(x => x)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key))
                    {
                        sb.AppendLine($@"Duplicate alias: ""{duplicateAlias}""");
                    }
                }
                sb.AppendLine();
            }

            var errors = sb.ToString();
            return errors;
        }

        private Task _runDiscordBotTask;
        private CancellationTokenSource _runDiscordBotCts;

        public async Task RunDiscordBot()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var config = scope.Resolve<IConfig>();
                var constants = scope.Resolve<IConstants>();
                var savedstate = scope.Resolve<ISavedState>();
                var skipExtract = savedstate.SkipExtractNextRestart;
                if (skipExtract)
                {
                    savedstate.SkipExtractNextRestart = false;
                    savedstate.Save();
                }

                var _bot = scope.Resolve<ArkDiscordBot>();
                await _bot.Initialize(_runDiscordBotCts.Token, skipExtract);
                var isConnected = false;
                var lastAttempt = DateTime.MinValue;
                var retryInterval = TimeSpan.FromSeconds(15);

                while (true)
                {
                    if (_runDiscordBotCts.IsCancellationRequested) break;

                    if (!isConnected && DateTime.Now - lastAttempt >= retryInterval)
                    {
                        try
                        {
                            lastAttempt = DateTime.Now;
                            System.Console.WriteLine("Connecting bot...");
                            await _bot.Start();
                            System.Console.WriteLine("Connected!");
                            isConnected = true;
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine($"Failed to connect ({ex.Message})! Will retry in a moment...");
                            Logging.LogException("Failed to start Discord Bot", ex, GetType(), Utils.LogLevel.DEBUG, ExceptionLevel.Ignored);
                        }
                    }

                    await Task.Delay(100);
                }

                await _bot.Stop();
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
                    try
                    {
                        Configuration?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logging.LogException(@"Exception in Workspace::Dispose (Configuration) when closing application", ex, GetType(), Utils.LogLevel.DEBUG, ExceptionLevel.Ignored);
                    }
                    Configuration = null;

                    try
                    {
                        About?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logging.LogException(@"Exception in Workspace::Dispose (About) when closing application", ex, GetType(), Utils.LogLevel.DEBUG, ExceptionLevel.Ignored);
                    }
                    About = null;

                    try
                    {
                        _webapp?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logging.LogException(@"Exception in Workspace::Dispose (WebApp) when closing application", ex, GetType(), Utils.LogLevel.DEBUG, ExceptionLevel.Ignored);
                    }
                    _webapp = null;
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
