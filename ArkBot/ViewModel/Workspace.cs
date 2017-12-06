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
using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

namespace ArkBot.ViewModel
{
    public class Workspace : ViewModelBase, IDisposable
    {
        public struct Constants
        {
            public const string ConfigFilePath = @"config.json";
            public const string LayoutFilePath = @".\Layout.config";
        }

        public static Workspace Instance => _instance ?? (_instance = new Workspace());
        private static Workspace _instance;

        public IEnumerable<PaneViewModel> Panes => _panes ?? (_panes = new PaneViewModel[] { Console });
        private PaneViewModel[] _panes;

        public ConsoleViewModel Console => _console ?? (_console = new ConsoleViewModel());
        private ConsoleViewModel _console;

        public DelegateCommand<System.ComponentModel.CancelEventArgs> ClosingCommand { get; private set; }

        public ICommand ExitCommand => _exitCommand ?? (_exitCommand = new RelayCommand(parameter => OnExit(parameter), parameter => CanExit(parameter)));
        private RelayCommand _exitCommand;

        public ICommand ReloadPartialConfig => _reloadPartialConfig ?? (_reloadPartialConfig = new RelayCommand(parameter => OnReloadPartialConfig(parameter), parameter => CanReloadPartialConfig(parameter)));
        private RelayCommand _reloadPartialConfig;

        public ObservableCollection<MenuItemViewModel> ManuallyUpdateServers { get; set; }
        public ObservableCollection<MenuItemViewModel> ManuallyUpdateClusters { get; set; }

        internal static IContainer Container { get; set; }

        public bool SkipExtractNextRestart
        {
            get
            {
                return _skipExtractNextRestart;
            }

            set
            {
                if (value == _skipExtractNextRestart) return;

                _skipExtractNextRestart = value;
                RaisePropertyChanged(nameof(SkipExtractNextRestart));
            }
        }
        private bool _skipExtractNextRestart;

        private SavedState _savedstate = null;
        private IDisposable _webapi;
        private IDisposable _webapp;
        private List<IDisposable> _webappRedirects;

        private ArkContextManager _contextManager;
        internal IConfig _config;

        public Workspace()
        {
            //do not create viewmodels or load data here, or avalondock layout deserialization will fail
            ManuallyUpdateServers = new ObservableCollection<MenuItemViewModel>();
            ManuallyUpdateClusters = new ObservableCollection<MenuItemViewModel>();
            ClosingCommand = new DelegateCommand<System.ComponentModel.CancelEventArgs>(OnClosing);
            PropertyChanged += Workspace_PropertyChanged;

            _webappRedirects = new List<IDisposable>();
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
                Task.WaitAny(_runDiscordBotTask);
            }
        }

        private bool CanExit(object parameter)
        {
            return true;
        }

        private void OnExit(object parameter)
        {
            Application.Current.MainWindow.Close();
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

            //load config and check for errors
            if (!File.Exists(Constants.ConfigFilePath))
            {
                WriteAndWaitForKey($@"The required file config.json is missing from application directory. Please copy defaultconfig.json, set the correct values for your environment and restart the application.");
                return;
            }

            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                WriteAndWaitForKey($@"This application must be run as administrator in order to function properly.");
                return;
            }

            _config = null;
            string exceptionMessage = null;
            try
            {
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Constants.ConfigFilePath));
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
            }
            if (_config == null)
            {
                WriteAndWaitForKey(
                    $@"The required file config.json is empty or contains errors. Please copy defaultconfig.json, set the correct values for your environment and restart the application.",
                    exceptionMessage);
                return;
            }

            var sb = new StringBuilder();
            if (string.IsNullOrWhiteSpace(_config.BotId) || !new Regex(@"^[a-z0-9]+$", RegexOptions.IgnoreCase | RegexOptions.Singleline).IsMatch(_config.BotId))
            {
                sb.AppendLine($@"Error: {nameof(_config.BotId)} is not a valid id.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.BotId))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.BotName))
            {
                sb.AppendLine($@"Error: {nameof(_config.BotName)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.BotName))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.BotNamespace) || !Uri.IsWellFormedUriString(_config.BotNamespace, UriKind.Absolute))
            {
                sb.AppendLine($@"Error: {nameof(_config.BotNamespace)} is not set or not a valid url.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.BotNamespace))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.TempFileOutputDirPath) || !Directory.Exists(_config.TempFileOutputDirPath))
            {
                sb.AppendLine($@"Error: {nameof(_config.TempFileOutputDirPath)} is not a valid directory path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.TempFileOutputDirPath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.BotToken))
            {
                sb.AppendLine($@"Error: {nameof(_config.BotToken)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.BotToken))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.SteamOpenIdRedirectUri))
            {
                sb.AppendLine($@"Error: {nameof(_config.SteamOpenIdRedirectUri)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.SteamOpenIdRedirectUri))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.SteamOpenIdRelyingServiceListenPrefix))
            {
                sb.AppendLine($@"Error: {nameof(_config.SteamOpenIdRelyingServiceListenPrefix)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.SteamOpenIdRelyingServiceListenPrefix))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.GoogleApiKey))
            {
                sb.AppendLine($@"Error: {nameof(_config.GoogleApiKey)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.GoogleApiKey))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.SteamApiKey))
            {
                sb.AppendLine($@"Error: {nameof(_config.SteamApiKey)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.SteamApiKey))}");
                sb.AppendLine();
            }
            if (_config.BackupsEnabled && (string.IsNullOrWhiteSpace(_config.BackupsDirectoryPath) || !FileHelper.IsValidDirectoryPath(_config.BackupsDirectoryPath)))
            {
                sb.AppendLine($@"Error: {nameof(_config.BackupsDirectoryPath)} is not a valid directory path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.BackupsDirectoryPath))}");
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
                if (!(_config.Ssl.Domains?.Length >= 1) || _config.Ssl.Domains.Any(x => string.IsNullOrWhiteSpace(x)))
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
            if (_config.Servers?.Length > 0)
            {
                foreach (var server in _config.Servers)
                {
                    if (server.Cluster != null && !clusterkeys.Contains(server.Cluster))
                    {
                        sb.AppendLine($@"Error: {nameof(_config.Servers)}.{nameof(server.Cluster)} reference missing cluster key ""{server.Cluster}"".");
                        sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(server, nameof(server.Cluster))}");
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
                    if (server.Port <= 0)
                    {
                        sb.AppendLine($@"Error: {nameof(_config.Servers)}.{nameof(server.Port)} is not valid for server instance ""{server.Key}"".");
                        sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(server, nameof(server.Port))}");
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
            if (_config.Clusters?.Length > 0)
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

            //todo: for now this section is not really needed unless !imprintcheck is used
            //if (config.ArkMultipliers == null)
            //{
            //    sb.AppendLine($@"Error: {nameof(config.ArkMultipliers)} section is missing from config file.");
            //    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.ArkMultipliers))}");
            //    sb.AppendLine();
            //}

            if (string.IsNullOrWhiteSpace(_config.AdminRoleName)) _config.AdminRoleName = "admin";
            if (string.IsNullOrWhiteSpace(_config.DeveloperRoleName)) _config.DeveloperRoleName = "developer";
            if (string.IsNullOrWhiteSpace(_config.MemberRoleName)) _config.DeveloperRoleName = "ark";

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

            var options = new SteamOpenIdOptions
            {
                ListenPrefixes = new[] { _config.SteamOpenIdRelyingServiceListenPrefix },
                RedirectUri = _config.SteamOpenIdRedirectUri,
            };
            var openId = new BarebonesSteamOpenId(options,
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
                        return service.RunCompile(html, constants.OpenidresponsetemplatePath, null, new { Success = success, botName = _config.BotName, botUrl = _config.BotUrl });
                    }
                }));

            //x =>
            //{
            //    x.LogLevel = LogSeverity.Warning;
            //    x.LogHandler += (s, e) => Console.AddLog(e.Message);
            //    x.AppName = _config.BotName;
            //    x.AppUrl = !string.IsNullOrWhiteSpace(_config.BotUrl) ? _config.BotUrl : null;
            //}


            var discord = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Warning
            });
            discord.Log += msg =>
            {
                Console.AddLog(msg.Message);
                return Task.CompletedTask;
            };

            var discordCommands = new CommandService(new CommandServiceConfig
            {
            });

            //setup dependency injection
            var thisAssembly = Assembly.GetExecutingAssembly();
            var builder = new ContainerBuilder();

            builder.RegisterType<ArkServerContext>().AsSelf();
            if (_config.UseCompatibilityChangeWatcher) builder.RegisterType<ArkSaveFileWatcherTimer>().As<IArkSaveFileWatcher>();
            else builder.RegisterType<ArkSaveFileWatcher>().As<IArkSaveFileWatcher>();
            builder.RegisterInstance(discord).AsSelf();
            builder.RegisterInstance(discordCommands).AsSelf();
            builder.RegisterType<AutofacDiscordServiceProvider>().As<IServiceProvider>().SingleInstance();
            builder.RegisterType<ArkDiscordBot>();
            builder.RegisterType<UrlShortenerService>().As<IUrlShortenerService>().SingleInstance();
            builder.RegisterInstance(constants).As<IConstants>();
            builder.RegisterInstance(_savedstate).As<ISavedState>();
            builder.RegisterInstance(_config as Config).As<IConfig>();
            //builder.RegisterInstance(playedTimeWatcher).As<IPlayedTimeWatcher>();
            builder.RegisterInstance(openId).As<IBarebonesSteamOpenId>();
            builder.RegisterType<EfDatabaseContext>().AsSelf().As<IEfDatabaseContext>()
                .WithParameter(new TypedParameter(typeof(string), constants.DatabaseConnectionString));
            builder.RegisterType<EfDatabaseContextFactory>();
            builder.RegisterType<Migrations.Configuration>().PropertiesAutowired();
            builder.RegisterType<ArkServerService>().As<IArkServerService>().SingleInstance();
            builder.RegisterType<SavegameBackupService>().As<ISavegameBackupService>().SingleInstance();
            builder.RegisterType<PlayerLastActiveService>().As<IPlayerLastActiveService>().SingleInstance();

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
            if (_config.Clusters?.Length > 0)
            {
                foreach (var cluster in _config.Clusters)
                {
                    var context = new ArkClusterContext(cluster);
                    _contextManager.AddCluster(context);
                }
            }

            if (_config.Servers?.Length > 0)
            {
                var playerLastActiveService = Container.Resolve<IPlayerLastActiveService>();
                var backupService = Container.Resolve<ISavegameBackupService>();
                foreach (var server in _config.Servers)
                {
                    var clusterContext = _contextManager.GetCluster(server.Cluster);
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
            if (_config.DiscordBotEnabled)
            {
                _runDiscordBotCts = new CancellationTokenSource();
                _runDiscordBotTask = await Task.Factory.StartNew(async () => await RunDiscordBot(), _runDiscordBotCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            else Console.AddLog("Discord bot is disabled.");

            //load the species stats data
            await ArkSpeciesStats.Instance.LoadOrUpdate();

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
                    try
                    {
                        using (var client = new AcmeClient(WellKnownServers.LetsEncrypt))
                        {
                            var account = await client.NewRegistraton($"mailto:{_config.Ssl.Email}");
                            account.Data.Agreement = account.GetTermsOfServiceUri();
                            account = await client.UpdateRegistration(account);

                            var authz = await client.NewAuthorization(new AuthorizationIdentifier
                            {
                                Type = AuthorizationIdentifierTypes.Dns,
                                Value = _config.Ssl.Domains.First()
                            });

                            var httpChallengeInfo = authz.Data.Challenges.Where(c => c.Type == ChallengeTypes.Http01).First();
                            var keyAuthString = client.ComputeKeyAuthorization(httpChallengeInfo);

                            using (var webapp = Microsoft.Owin.Hosting.WebApp.Start(_config.Ssl.ChallengeListenPrefix, (appBuilder) =>
                            {
                                var challengePath = new PathString("/.well-known/acme-challenge/");
                                appBuilder.Use(new Func<AppFunc, AppFunc>((next) =>
                                {
                                    AppFunc appFunc = async environment =>
                                    {
                                        IOwinContext context = new OwinContext(environment);
                                        if (!context.Request.Path.Equals(challengePath)) await next.Invoke(environment);

                                        context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
                                        context.Response.ContentType = "application/text";
                                        await context.Response.WriteAsync(keyAuthString);
                                    };
                                    return appFunc;
                                }));
                            }))
                            {
                                var httpChallenge = await client.CompleteChallenge(httpChallengeInfo);

                                authz = await client.GetAuthorization(httpChallenge.Location);
                                while (authz.Data.Status == EntityStatus.Pending)
                                {
                                    await Task.Delay(500);
                                    authz = await client.GetAuthorization(httpChallenge.Location);
                                }

                                if (authz.Data.Status == EntityStatus.Valid)
                                {
                                    if (revoke)
                                    {
                                        //await client.RevokeCertificate(path); //todo: how to revoke a cert when we do not have the AcmeCertificate-object?
                                        if (File.Exists(path)) File.Delete(path);
                                    }

                                    var csr = new CertificationRequestBuilder();
                                    foreach (var domain in _config.Ssl.Domains) csr.AddName("CN", domain);
                                    var cert = await client.NewCertificate(csr);

                                    var pfxBuilder = cert.ToPfx();
                                    var pfx = pfxBuilder.Build(_config.Ssl.Name, _config.Ssl.Password);
                                    File.WriteAllBytes(path, pfx);

                                    success = true;
                                }
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        _webapi?.Dispose();
                    } catch (ObjectDisposedException) { }
                    _webapi = null;

                    try
                    {
                        _webapp?.Dispose();
                    }
                    catch (ObjectDisposedException) { }
                    _webapp = null;

                    foreach (var redir in _webappRedirects.ToArray())
                    {
                        try
                        {
                            redir?.Dispose();
                        }
                        catch (ObjectDisposedException) { }
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
