using ArkBot.Ark;
using ArkBot.Data;
using ArkBot.Database;
using ArkBot.Database.Model;
using ArkBot.Discord;
using ArkBot.Helpers;
using ArkBot.Notifications;
using ArkBot.OpenID;
using ArkBot.ScheduledTasks;
using ArkBot.Services;
using ArkBot.Voting;
using ArkBot.Voting.Handlers;
using ArkBot.WebApi;
using ArkBot.WpfCommands;
using Autofac;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Certes;
using Certes.Acme;
using Certes.Pkcs;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Prism.Commands;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ArkSavegameToolkitNet.Domain;
using Discord.Net.Providers.WS4Net;
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
using Nito.AsyncEx;
using Markdig;
using PropertyChanged;
using ArkBot.Configuration.Model;
using System.Globalization;
using ArkSavegameToolkitNet;

namespace ArkBot.ViewModel
{
    public sealed class Workspace : ViewModelBase, IDisposable
    {
        public struct Constants
        {
            public const string ConfigFilePath = @"config.json";
            public const string ArkConfigFilePath = @"ark.json";
            public const string DefaultConfigFilePath = @"defaultconfig.json";
            public const string LayoutFilePath = @".\Layout.config";
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
        private IDisposable _webapi;
        private IDisposable _webapp;
        private List<IDisposable> _webappRedirects;

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
            //Bind both the show or hide command and task bar double click to the same action
            ShowOrHideCommand = new DelegateCommand(OnShowHide);
            TaskIconDoubleClickCommand = new DelegateCommand(OnShowHide);

            PropertyChanged += Workspace_PropertyChanged;

            _webappRedirects = new List<IDisposable>();

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

        private static Task<Workspace> CreateAsync()
        {
            _instance = new Workspace();
            return _instance.InitializeAsync();
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

        private void OnShowHide()
        {
            //If the UI is hidden, show the main window
            if (_isUIHidden && Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow?.Show();
                //Make sure the app has the focus
                Application.Current.MainWindow.Topmost = true;
                Application.Current.MainWindow.Topmost = false;
                //Set the variable to indicate if the application should be shown
                _isUIHidden = false;
            }
            else if (!_isUIHidden && Application.Current.MainWindow != null) //If the UI is shown, hide the main window
            {
                Application.Current.MainWindow?.Hide();
                //Set the variable to indicate if the application should be hidden
                _isUIHidden = true;
            }
        }

        private bool CanExit(object parameter)
        {
            return true;
        }

        private void OnExit(object parameter)
        {
            Application.Current.MainWindow?.Close();
        }

        private bool CanOpenWebApp(object parameter)
        {
            return !string.IsNullOrEmpty(_config?.AppUrl);
        }

        private void OnOpenWebApp(object parameter)
        {
            Process.Start(_config.AppUrl);
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
            _config.AccessControl = config.AccessControl;
            _config.UserRoles = config.UserRoles;
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

            log4net.Config.XmlConfigurator.Configure();

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
                    Logging.LogException("Error loading 'ark.config'. Using default config.", ex, this.GetType());
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

            var constants = new ArkBot.Constants();

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
            //var context = new ArkContext(config, constants, progress);

            //var playedTimeWatcher = new PlayedTimeWatcher(_config);

            BarebonesSteamOpenId openId = null;
            if (_config.Discord.DiscordBotEnabled)
            {
                var options = new SteamOpenIdOptions
                {
                    ListenPrefixes = new[] { _config.Discord.SteamOpenIdRelyingServiceListenPrefix },
                    RedirectUri = _config.Discord.SteamOpenIdRedirectUri,
                };
                openId = new BarebonesSteamOpenId(options,
                    new Func<bool, ulong, ulong, Task<string>>(async (success, steamId, discordId) =>
                    {
                        var razorConfig = new TemplateServiceConfiguration
                        {
                            DisableTempFileLocking = true,
                            CachingProvider = new DefaultCachingProvider(t => { })
                        };

                        using (var service = RazorEngineService.Create(razorConfig))
                        {
                            var html = await FileHelper.ReadAllTextTaskAsync(constants.OpenidresponsetemplatePath);
                            return service.RunCompile(html, constants.OpenidresponsetemplatePath, null,
                                new { Success = success, botName = _config.BotName, botUrl = _config.BotUrl });
                        }
                    }));
            }

            var discord = new DiscordSocketClient(new DiscordSocketConfig
            {
                WebSocketProvider = WS4NetProvider.Instance, //required for Win 7
                LogLevel = _config.DiscordLogLevel
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
            builder.RegisterType<WebApp.SinglePageApplicationModule>().AsSelf();
            builder.RegisterInstance(constants).As<IConstants>();
            builder.RegisterInstance(_savedstate).As<ISavedState>();
            builder.RegisterInstance(_config as Config).As<IConfig>();
            //builder.RegisterInstance(playedTimeWatcher).As<IPlayedTimeWatcher>();
            if (openId != null) builder.RegisterInstance(openId).As<IBarebonesSteamOpenId>();
            builder.RegisterType<EfDatabaseContext>().AsSelf().As<IEfDatabaseContext>()
                .WithParameter(new TypedParameter(typeof(string), constants.DatabaseConnectionString));
            builder.RegisterType<EfDatabaseContextFactory>();
            builder.RegisterType<Migrations.Configuration>().PropertiesAutowired();
            builder.RegisterType<ArkServerService>().As<IArkServerService>().SingleInstance();
            builder.RegisterType<SavegameBackupService>().As<ISavegameBackupService>().SingleInstance();
            builder.RegisterType<PlayerLastActiveService>().As<IPlayerLastActiveService>().SingleInstance();
            builder.RegisterType<LogCleanupService>().As<ILogCleanupService>().SingleInstance();

            //register vote handlers
            builder.RegisterType<BanVoteHandler>().As<IVoteHandler<BanVote>>();
            builder.RegisterType<UnbanVoteHandler>().As<IVoteHandler<UnbanVote>>();
            builder.RegisterType<RestartServerVoteHandler>().As<IVoteHandler<RestartServerVote>>();
            builder.RegisterType<UpdateServerVoteHandler>().As<IVoteHandler<UpdateServerVote>>();
            builder.RegisterType<DestroyWildDinosVoteHandler>().As<IVoteHandler<DestroyWildDinosVote>>();
            builder.RegisterType<SetTimeOfDayVoteHandler>().As<IVoteHandler<SetTimeOfDayVote>>();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterHubs(Assembly.GetExecutingAssembly());
            builder.RegisterType<ArkContextManager>().WithParameter(new TypedParameter(typeof(IProgress<string>), progress)).AsSelf().SingleInstance();
            builder.RegisterType<VotingManager>().WithParameter(new TypedParameter(typeof(IProgress<string>), progress)).AsSelf().SingleInstance();
            builder.RegisterType<DiscordManager>().AsSelf().SingleInstance();
            builder.RegisterType<ScheduledTasksManager>().AsSelf().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies).SingleInstance();
            builder.RegisterType<NotificationManager>().AsSelf().SingleInstance();

            builder.RegisterType<AutofacDependencyResolver>().As<IDependencyResolver>().SingleInstance();

            builder.RegisterType<WebApiStartup>().AsSelf();
            builder.RegisterType<WebApp.WebAppStartup>().AsSelf();

            var webapiConfig = new System.Web.Http.HttpConfiguration();
            var webappConfig = new System.Web.Http.HttpConfiguration();
            builder.RegisterInstance(webapiConfig).Keyed<System.Web.Http.HttpConfiguration>("webapi");
            builder.RegisterInstance(webappConfig).Keyed<System.Web.Http.HttpConfiguration>("webapp");

            builder.RegisterWebApiFilterProvider(webapiConfig);
            builder.Register(c => new AccessControlAuthorizationFilter(c.Resolve<IConfig>()))
                .AsWebApiAuthorizationFilterFor<WebApi.Controllers.BaseApiController>()
                .InstancePerRequest();

            //kernel.Bind(typeof(IHubConnectionContext<dynamic>)).ToMethod(context =>
            //        resolver.Resolve<IConnectionManager>().GetHubContext<StockTickerHub>().Clients
            //         ).WhenInjectedInto<IStockTicker>();

            Container = builder.Build();

            var dir = Path.GetDirectoryName(constants.DatabaseFilePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            //update database
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<EfDatabaseContext, Migrations.Configuration>(true, Container.Resolve<Migrations.Configuration>()));

            //create database immediately to support direct (non-ef) access in application
            using (var db = Container.Resolve<IEfDatabaseContext>())
            {
                db.Database.Initialize(false);
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
                var votingManager = Container.Resolve<VotingManager>();
                var notificationMangager = Container.Resolve<NotificationManager>();

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
            }

            //run the discord bot
            if (_config.Discord.DiscordBotEnabled)
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
            if (_config.Ssl?.Enabled == true)
            {
                var path = $"{_config.Ssl.Name}.pfx";
                var revoke = false;
                var renew = false;
                if (File.Exists(path))
                {
                    try
                    {
                        using (var rlt = new X509Certificate2(path, _config.Ssl.Password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet))
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
                    catch (Exception ex) { Logging.LogException("Failed to remove ssl certificate from store.", ex, this.GetType()); }
                }
                else renew = true;

                if (renew)
                {
                    var success = false;
                    Console.AddLog(@"SSL Certificate request issued...");
                    AcmeContext acme = null;
                    try
                    {
                        var pathAccountKey = $"{_config.Ssl.Name}-account.pem";
                        //var pathPrivateKey = $"{_config.Ssl.Name}-privatekey.pem";
                        if (File.Exists(pathAccountKey))
                        {
                            var accountKey = KeyFactory.FromPem(File.ReadAllText(pathAccountKey));

                            acme = new AcmeContext(WellKnownServers.LetsEncryptV2, accountKey);
                            var account = await acme.Account();
                        }
                        else
                        {
                            acme = new AcmeContext(WellKnownServers.LetsEncryptV2);
                            var account = await acme.NewAccount(_config.Ssl.Email, true);

                            var pemKey = acme.AccountKey.ToPem();
                            File.WriteAllText(pathAccountKey, pemKey);
                        }

                        var order = await acme.NewOrder(_config.Ssl.Domains);

                        var authz = (await order.Authorizations()).First();
                        var httpChallenge = await authz.Http();
                        var keyAuthz = httpChallenge.KeyAuthz;
                        using (var webapp = Microsoft.Owin.Hosting.WebApp.Start(_config.Ssl.ChallengeListenPrefix, (appBuilder) =>
                        {
                            var challengePath = new PathString($"/.well-known/acme-challenge/{httpChallenge.Token}");
                            appBuilder.Use(new Func<AppFunc, AppFunc>((next) =>
                            {
                                AppFunc appFunc = async environment =>
                                {
                                    IOwinContext context = new OwinContext(environment);
                                    if (!context.Request.Path.Equals(challengePath)) await next.Invoke(environment);

                                    context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                                    context.Response.ContentType = "application/text";
                                    await context.Response.WriteAsync(keyAuthz);
                                };
                                return appFunc;
                            }));
                        }))
                        {
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
                                var pfx = pfxBuilder.Build(_config.Ssl.Name, _config.Ssl.Password);
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
                        Logging.LogException("Failed to issue ssl certificate.", ex, this.GetType());
                    }
                }

                if (File.Exists(path))
                {
                    var hostname = _config.Ssl.Domains.First();

                    var attribute = (GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0];
                    var appId = attribute.Value;

                    try
                    {
                        using (var rlt = new X509Certificate2(path, _config.Ssl.Password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet))
                        {
                            using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                            {
                                store.Open(OpenFlags.ReadWrite);

                                var certs = store.Certificates.Find(X509FindType.FindBySubjectName, _config.Ssl.Domains.First(), false);

                                if (!store.Certificates.Contains(rlt)) store.Add(rlt);
                                store.Close();
                            }

                            if (_config.Ssl.UseCompatibilityNonSNIBindings) Console.AddLog(@"Binding SSL Certificate to ip/port...");
                            else Console.AddLog(@"Binding SSL Certificate to hostname/port...");
                            foreach (var port in _config.Ssl.Ports)
                            {
                                var commands = new[]
                                {
                                _config.Ssl.UseCompatibilityNonSNIBindings ? $"netsh http delete sslcert ipport=0.0.0.0:{port}" : $"netsh http delete sslcert hostnameport={hostname}:{port}",
                                _config.Ssl.UseCompatibilityNonSNIBindings ? $"netsh http add sslcert ipport=0.0.0.0:{port} certhash={rlt.Thumbprint} appid={{{appId}}} certstore=my" : $"netsh http add sslcert hostnameport={hostname}:{port} certhash={rlt.Thumbprint} appid={{{appId}}} certstore=my"
                            };

                                var exitCode = 0;
                                foreach (var cmd in commands)
                                {
                                    await Task.Run(() =>
                                    {
                                        using (var proc = Process.Start(new ProcessStartInfo
                                        {
                                            FileName = "cmd.exe",
                                            Arguments = $"/c {cmd}",
                                            Verb = "runas",
                                            UseShellExecute = false,
                                            WindowStyle = ProcessWindowStyle.Hidden,
                                            CreateNoWindow = true
                                        }))
                                        {
                                            proc.WaitForExit();
                                            exitCode = proc.ExitCode; //only add (last cmd) is interesting
                                        }
                                    });
                                }

                                if (_config.Ssl.UseCompatibilityNonSNIBindings) Console.AddLog("[" + (exitCode == 0 ? "Success" : "Failed") + $"] ipport: 0.0.0.0:{port}, thumbprint={rlt.Thumbprint}, appid={{{appId}}}");
                                else Console.AddLog("[" + (exitCode == 0 ? "Success" : "Failed") + $"] hostnameport: {hostname}:{port}, thumbprint={rlt.Thumbprint}, appid={{{appId}}}");
                            }
                        }
                    }
                    catch (CryptographicException ex)
                    {
                        Logging.LogException("Failed to open SSL certificate.", ex, this.GetType(), LogLevel.FATAL, ExceptionLevel.Unhandled);
                        WriteAndWaitForKey("Failed to open SSL certificate (wrong password?)");
                        return;
                    }
                }
            }

            //webapi
            _webapi = Microsoft.Owin.Hosting.WebApp.Start(_config.WebApiListenPrefix, app =>
            {
                var startup = Container.Resolve<WebApiStartup>();
                startup.Configuration(app, _config, Container, webapiConfig);
            });
            Console.AddLog("Web API started");



            //webapp
            _webapp = Microsoft.Owin.Hosting.WebApp.Start(_config.WebAppListenPrefix, app =>
            {
                var startup = Container.Resolve<WebApp.WebAppStartup>();
                startup.Configuration(app, _config, Container, webappConfig);
            });
            Console.AddLog("Web App started");

            if (_config.WebAppRedirectListenPrefix?.Length > 0)
            {
                foreach (var redir in _config.WebAppRedirectListenPrefix)
                {
                    _webappRedirects.Add(Microsoft.Owin.Hosting.WebApp.Start<WebApp.WebAppRedirectStartup>(url: redir));
                    Console.AddLog("Web App redirect added");
                }
            }

            //Indicates the application started without errors, required for auto hide on startup
            _startedWithoutErrors = true;

        }

        private string ValidateConfig()
        {
            var sb = new StringBuilder();
            if (string.IsNullOrWhiteSpace(_config.BotName))
            {
                sb.AppendLine($@"Error: {nameof(_config.BotName)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.BotName))}");
                sb.AppendLine();
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
            if (_config.Backups.BackupsEnabled && (string.IsNullOrWhiteSpace(_config.Backups.BackupsDirectoryPath) || !FileHelper.IsValidDirectoryPath(_config.Backups.BackupsDirectoryPath)))
            {
                sb.AppendLine($@"Error: {nameof(_config.Backups.BackupsDirectoryPath)} is not a valid directory path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.Backups.BackupsDirectoryPath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.WebApiListenPrefix))
            {
                sb.AppendLine($@"Error: {nameof(_config.WebApiListenPrefix)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.WebApiListenPrefix))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.WebAppListenPrefix))
            {
                sb.AppendLine($@"Error: {nameof(_config.WebAppListenPrefix)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.WebAppListenPrefix))}");
                sb.AppendLine();
            }
            if (_config.Ssl?.Enabled == true)
            {
                if (string.IsNullOrWhiteSpace(_config.Ssl.Name))
                {
                    sb.AppendLine($@"Error: {nameof(_config.Ssl)}.{nameof(_config.Ssl.Name)} is not set.");
                    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config.Ssl, nameof(_config.Ssl.Name))}");
                    sb.AppendLine();
                }
                if (string.IsNullOrWhiteSpace(_config.Ssl.Password))
                {
                    sb.AppendLine($@"Error: {nameof(_config.Ssl)}.{nameof(_config.Ssl.Password)} is not set.");
                    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config.Ssl, nameof(_config.Ssl.Password))}");
                    sb.AppendLine();
                }
                if (string.IsNullOrWhiteSpace(_config.Ssl.Email))
                {
                    sb.AppendLine($@"Error: {nameof(_config.Ssl)}.{nameof(_config.Ssl.Email)} is not set.");
                    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config.Ssl, nameof(_config.Ssl.Email))}");
                    sb.AppendLine();
                }
                if (!(_config.Ssl.Domains?.Count >= 1) || _config.Ssl.Domains.Any(x => string.IsNullOrWhiteSpace(x)))
                {
                    sb.AppendLine($@"Error: {nameof(_config.Ssl)}.{nameof(_config.Ssl.Domains)} is not set.");
                    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config.Ssl, nameof(_config.Ssl.Domains))}");
                    sb.AppendLine();
                }
                if (string.IsNullOrWhiteSpace(_config.Ssl.ChallengeListenPrefix))
                {
                    sb.AppendLine($@"Error: {nameof(_config.Ssl)}.{nameof(_config.Ssl.ChallengeListenPrefix)} is not set.");
                    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config.Ssl, nameof(_config.Ssl.ChallengeListenPrefix))}");
                    sb.AppendLine();
                }
            }
            if (_config.Discord.DiscordBotEnabled)
            {
                if (string.IsNullOrWhiteSpace(_config.Discord.BotToken))
                {
                    sb.AppendLine($@"Error: {nameof(_config.Discord)}.{nameof(_config.Discord.BotToken)} is not set.");
                    sb.AppendLine($@"Expected value: {
                            ValidationHelper.GetDescriptionForMember(_config.Discord, nameof(_config.Discord.BotToken))
                        }");
                    sb.AppendLine();
                }
                if (string.IsNullOrWhiteSpace(_config.Discord.SteamOpenIdRedirectUri))
                {
                    sb.AppendLine($@"Error: {nameof(_config.Discord)}.{nameof(_config.Discord.SteamOpenIdRedirectUri)} is not set.");
                    sb.AppendLine($@"Expected value: {
                            ValidationHelper.GetDescriptionForMember(_config.Discord,
                                nameof(_config.Discord.SteamOpenIdRedirectUri))
                        }");
                    sb.AppendLine();
                }
                if (string.IsNullOrWhiteSpace(_config.Discord.SteamOpenIdRelyingServiceListenPrefix))
                {
                    sb.AppendLine(
                        $@"Error: {nameof(_config.Discord)}.{nameof(_config.Discord.SteamOpenIdRelyingServiceListenPrefix)} is not set.");
                    sb.AppendLine($@"Expected value: {
                            ValidationHelper.GetDescriptionForMember(_config.Discord,
                                nameof(_config.Discord.SteamOpenIdRelyingServiceListenPrefix))
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
                    //if (server.RconPort <= 0)
                    //{
                    //    sb.AppendLine($@"Error: {nameof(config.Servers)}.{nameof(server.RconPort)} is not valid for server instance ""{server.Key}"".");
                    //    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(server, nameof(server.RconPort))}");
                    //    sb.AppendLine();
                    //}
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

            //return;

            //todo: for now this section is not really needed unless !imprintcheck is used
            //if (config.ArkMultipliers == null)
            //{
            //    sb.AppendLine($@"Error: {nameof(config.ArkMultipliers)} section is missing from config file.");
            //    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.ArkMultipliers))}");
            //    sb.AppendLine();
            //}

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

                    if (!isConnected && (DateTime.Now - lastAttempt) >= retryInterval)
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
                            Logging.LogException("Failed to start Discord Bot", ex, GetType(), LogLevel.DEBUG, ExceptionLevel.Ignored);
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
                        Logging.LogException(@"Exception in Workspace::Dispose (Configuration) when closing application", ex, GetType(), LogLevel.DEBUG, ExceptionLevel.Ignored);
                    }
                    Configuration = null;

                    try
                    {
                        About?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logging.LogException(@"Exception in Workspace::Dispose (About) when closing application", ex, GetType(), LogLevel.DEBUG, ExceptionLevel.Ignored);
                    }
                    About = null;

                    try
                    {
                        _webapi?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logging.LogException(@"Exception in Workspace::Dispose (WebApi) when closing application", ex, GetType(), LogLevel.DEBUG, ExceptionLevel.Ignored);
                    }
                    _webapi = null;

                    try
                    {
                        _webapp?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logging.LogException(@"Exception in Workspace::Dispose (WebApp) when closing application", ex, GetType(), LogLevel.DEBUG, ExceptionLevel.Ignored);
                    }
                    _webapp = null;

                    foreach (var redir in _webappRedirects.ToArray())
                    {
                        try
                        {
                            redir?.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Logging.LogException(@"Exception in Workspace::Dispose (WebAppRedirects) when closing application", ex, GetType(), LogLevel.DEBUG, ExceptionLevel.Ignored);
                        }
                        _webappRedirects.Remove(redir);
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
