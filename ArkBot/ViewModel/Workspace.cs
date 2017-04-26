using ArkBot.Ark;
using ArkBot.Data;
using ArkBot.Database;
using ArkBot.Database.Model;
using ArkBot.Helpers;
using ArkBot.OpenID;
using ArkBot.Services;
using ArkBot.Vote;
using ArkBot.WebApi;
using ArkBot.WpfCommands;
using Autofac;
using Autofac.Integration.WebApi;
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ArkBot.ViewModel
{
    public class Workspace : ViewModelBase, IDisposable
    {
        public struct Constants
        {
            public const string LayoutFilePath = @".\Layout.config";
        }

        public static Workspace Instance => _instance ?? (_instance = new Workspace());
        private static Workspace _instance;

        public IEnumerable<PaneViewModel> Panes => _panes ?? (_panes = new PaneViewModel[] { Console });
        private PaneViewModel[] _panes;

        public ConsoleViewModel Console => _console ?? (_console = new ConsoleViewModel());
        private ConsoleViewModel _console;

        public ICommand ExitCommand => _exitCommand ?? (_exitCommand = new RelayCommand(parameter => OnExit(parameter), parameter => CanExit(parameter)));
        private RelayCommand _exitCommand;

        public DelegateCommand<System.ComponentModel.CancelEventArgs> ClosingCommand { get; private set; }

        internal static IContainer Container { get; set; }

        public Dictionary<string, ArkServerContext> ServerContexts { get { return _serverContexts; } }
        private Dictionary<string, ArkServerContext> _serverContexts = new Dictionary<string, ArkServerContext>(StringComparer.OrdinalIgnoreCase);

        public Dictionary<string, ArkClusterContext> ClusterContexts { get { return _clusterContexts; } }
        private Dictionary<string, ArkClusterContext> _clusterContexts = new Dictionary<string, ArkClusterContext>(StringComparer.OrdinalIgnoreCase);


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

        public Workspace()
        {
            //do not create viewmodels or load data here, or avalondock layout deserialization will fail
            ClosingCommand = new DelegateCommand<System.ComponentModel.CancelEventArgs>(OnClosing);
            PropertyChanged += Workspace_PropertyChanged;
        }

        private void Workspace_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SkipExtractNextRestart) && _savedstate != null)
            {
                _savedstate.SkipExtractNextRestart = SkipExtractNextRestart;
                _savedstate.Save();
            }
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

        static void WriteAndWaitForKey(params string[] msgs)
        {
            foreach (var msg in msgs) if (msg != null) System.Console.WriteLine(msg);
        }

        internal async Task Init()
        {
            System.Console.WriteLine("ARK Discord Bot");
            System.Console.WriteLine("------------------------------------------------------");
            System.Console.WriteLine();

            //load config and check for errors
            var configPath = @"config.json";
            if (!File.Exists(configPath))
            {
                WriteAndWaitForKey($@"The required file config.json is missing form application directory. Please copy defaultconfig.json, set the correct values for your environment and restart the application.");
                return;
            }

            Config config = null;
            string exceptionMessage = null;
            try
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
            }
            if (config == null)
            {
                WriteAndWaitForKey(
                    $@"The required file config.json is empty or contains errors. Please copy defaultconfig.json, set the correct values for your environment and restart the application.",
                    exceptionMessage);
                return;
            }

            var sb = new StringBuilder();
            if (string.IsNullOrWhiteSpace(config.BotId) || !new Regex(@"^[a-z0-9]+$", RegexOptions.IgnoreCase | RegexOptions.Singleline).IsMatch(config.BotId))
            {
                sb.AppendLine($@"Error: {nameof(config.BotId)} is not a valid id.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.BotId))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.BotName))
            {
                sb.AppendLine($@"Error: {nameof(config.BotName)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.BotName))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.BotNamespace) || !Uri.IsWellFormedUriString(config.BotNamespace, UriKind.Absolute))
            {
                sb.AppendLine($@"Error: {nameof(config.BotNamespace)} is not set or not a valid url.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.BotNamespace))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.SaveFilePath) || !File.Exists(config.SaveFilePath))
            {
                sb.AppendLine($@"Error: {nameof(config.SaveFilePath)} is not a valid file path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.SaveFilePath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.ClusterSavePath) || !Directory.Exists(config.ClusterSavePath))
            {
                sb.AppendLine($@"Error: {nameof(config.ClusterSavePath)} is not a valid directory path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.ClusterSavePath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.ArktoolsExecutablePath) || !File.Exists(config.ArktoolsExecutablePath))
            {
                sb.AppendLine($@"Error: {nameof(config.ArktoolsExecutablePath)} is not a valid file path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.ArktoolsExecutablePath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.JsonOutputDirPath) || !Directory.Exists(config.JsonOutputDirPath))
            {
                sb.AppendLine($@"Error: {nameof(config.JsonOutputDirPath)} is not a valid directory path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.JsonOutputDirPath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.TempFileOutputDirPath) || !Directory.Exists(config.TempFileOutputDirPath))
            {
                sb.AppendLine($@"Error: {nameof(config.TempFileOutputDirPath)} is not a valid directory path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.TempFileOutputDirPath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.BotToken))
            {
                sb.AppendLine($@"Error: {nameof(config.BotToken)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.BotToken))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.SteamOpenIdRedirectUri))
            {
                sb.AppendLine($@"Error: {nameof(config.SteamOpenIdRedirectUri)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.SteamOpenIdRedirectUri))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.SteamOpenIdRelyingServiceListenPrefix))
            {
                sb.AppendLine($@"Error: {nameof(config.SteamOpenIdRelyingServiceListenPrefix)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.SteamOpenIdRelyingServiceListenPrefix))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.GoogleApiKey))
            {
                sb.AppendLine($@"Error: {nameof(config.GoogleApiKey)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.GoogleApiKey))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.SteamApiKey))
            {
                sb.AppendLine($@"Error: {nameof(config.SteamApiKey)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.SteamApiKey))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.ServerIp))
            {
                sb.AppendLine($@"Error: {nameof(config.ServerIp)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.ServerIp))}");
                sb.AppendLine();
            }
            if (config.ServerPort <= 0)
            {
                sb.AppendLine($@"Error: {nameof(config.ServerPort)} is not valid.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.ServerPort))}");
                sb.AppendLine();
            }
            if (config.BackupsEnabled && (string.IsNullOrWhiteSpace(config.BackupsDirectoryPath) || !FileHelper.IsValidDirectoryPath(config.BackupsDirectoryPath)))
            {
                sb.AppendLine($@"Error: {nameof(config.BackupsDirectoryPath)} is not a valid directory path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.BackupsDirectoryPath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(config.WebApiListenPrefix))
            {
                sb.AppendLine($@"Error: {nameof(config.WebApiListenPrefix)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.WebApiListenPrefix))}");
                sb.AppendLine();
            }

            var clusterkeys = config.Clusters?.Select(x => x.Key).ToArray();
            var serverkeys = config.Servers?.Select(x => x.Key).ToArray();
            if (serverkeys?.Length > 0 && serverkeys.Length != serverkeys.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                sb.AppendLine($@"Error: {nameof(config.Servers)} contain non-unique keys.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.Servers))}");
                sb.AppendLine();
            }
            if (clusterkeys?.Length > 0 && clusterkeys.Length != clusterkeys.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                sb.AppendLine($@"Error: {nameof(config.Clusters)} contain non-unique keys.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.Clusters))}");
                sb.AppendLine();
            }
            if (config.Servers?.Length > 0)
            {
                foreach (var server in config.Servers)
                {
                    if (server.Cluster != null && !clusterkeys.Contains(server.Cluster))
                    {
                        sb.AppendLine($@"Error: {nameof(config.Servers)}.{nameof(server.Cluster)} reference missing cluster key ""{server.Cluster}"".");
                        sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(server, nameof(server.Cluster))}");
                        sb.AppendLine();
                    }
                    if (string.IsNullOrWhiteSpace(server.SaveFilePath) || !File.Exists(server.SaveFilePath))
                    {
                        sb.AppendLine($@"Error: {nameof(config.Servers)}.{nameof(server.SaveFilePath)} is not a valid file path for server instance ""{server.Key}"".");
                        sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(server, nameof(server.SaveFilePath))}");
                        sb.AppendLine();
                    }
                    if (string.IsNullOrWhiteSpace(server.Ip))
                    {
                        sb.AppendLine($@"Error: {nameof(config.Servers)}.{nameof(server.Ip)} is not set for server instance ""{server.Key}"".");
                        sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(server, nameof(server.Ip))}");
                        sb.AppendLine();
                    }
                    if (server.Port <= 0)
                    {
                        sb.AppendLine($@"Error: {nameof(config.Servers)}.{nameof(server.Port)} is not valid for server instance ""{server.Key}"".");
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
            if (config.Clusters?.Length > 0)
            {
                foreach (var cluster in config.Clusters)
                {
                    if (string.IsNullOrWhiteSpace(cluster.SavePath) || !Directory.Exists(cluster.SavePath))
                    {
                        sb.AppendLine($@"Error: {nameof(config.Servers)}.{nameof(cluster.SavePath)} is not a valid directory path for cluster instance ""{cluster.Key}"".");
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

            if (string.IsNullOrWhiteSpace(config.AdminRoleName)) config.AdminRoleName = "admin";
            if (string.IsNullOrWhiteSpace(config.DeveloperRoleName)) config.DeveloperRoleName = "developer";
            if (string.IsNullOrWhiteSpace(config.MemberRoleName)) config.DeveloperRoleName = "ark";

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
                System.Console.WriteLine(message);
            });

            var constants = new ArkBot.Constants();

            if (config.Debug)
            {
                //we reset the state so that every run will be the same
                if (File.Exists(constants.DatabaseFilePath)) File.Delete(constants.DatabaseFilePath);
                if (File.Exists(constants.SavedStateFilePath)) File.Delete(constants.SavedStateFilePath);

                //optionally use a saved database state
                var databaseStateFilePath = Path.Combine(config.JsonOutputDirPath, "Database.state");
                if (File.Exists(databaseStateFilePath)) File.Copy(databaseStateFilePath, constants.DatabaseFilePath);
            }

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

            var playedTimeWatcher = new PlayedTimeWatcher(config);

            var options = new SteamOpenIdOptions
            {
                ListenPrefixes = new[] { config.SteamOpenIdRelyingServiceListenPrefix },
                RedirectUri = config.SteamOpenIdRedirectUri,
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
                        return service.RunCompile(html, constants.OpenidresponsetemplatePath, null, new { Success = success, botName = config.BotName, botUrl = config.BotUrl });
                    }
                }));

            //setup dependency injection
            var thisAssembly = Assembly.GetExecutingAssembly();
            var builder = new ContainerBuilder();
            builder.RegisterType<ArkDiscordBot>();
            builder.RegisterType<UrlShortenerService>().As<IUrlShortenerService>().SingleInstance();
            builder.RegisterInstance(constants).As<IConstants>();
            builder.RegisterInstance(_savedstate).As<ISavedState>();
            builder.RegisterInstance(config).As<IConfig>();
            builder.RegisterInstance(playedTimeWatcher).As<IPlayedTimeWatcher>();
            builder.RegisterType<ArkContext>().As<IArkContext>()
                .WithParameter(new TypedParameter(typeof(IProgress<string>), progress)).SingleInstance();
            builder.RegisterAssemblyTypes(thisAssembly).As<ArkBot.Commands.ICommand>().AsSelf().SingleInstance()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            builder.RegisterInstance(openId).As<IBarebonesSteamOpenId>();
            builder.RegisterType<EfDatabaseContext>().AsSelf().As<IEfDatabaseContext>()
                .WithParameter(new TypedParameter(typeof(string), constants.DatabaseConnectionString));
            builder.RegisterType<EfDatabaseContextFactory>();
            builder.RegisterType<Migrations.Configuration>().PropertiesAutowired();
            builder.RegisterType<ArkServerService>().As<IArkServerService>().SingleInstance();
            builder.RegisterType<SavegameBackupService>().As<ISavegameBackupService>().SingleInstance();

            //register vote handlers
            builder.RegisterType<BanVoteHandler>().As<IVoteHandler<BanVote>>();
            builder.RegisterType<UnbanVoteHandler>().As<IVoteHandler<UnbanVote>>();
            builder.RegisterType<RestartServerVoteHandler>().As<IVoteHandler<RestartServerVote>>();
            builder.RegisterType<UpdateServerVoteHandler>().As<IVoteHandler<UpdateServerVote>>();
            builder.RegisterType<DestroyWildDinosVoteHandler>().As<IVoteHandler<DestroyWildDinosVote>>();
            builder.RegisterType<SetTimeOfDayVoteHandler>().As<IVoteHandler<SetTimeOfDayVote>>();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            Container = builder.Build();

            //update database
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<EfDatabaseContext, Migrations.Configuration>(true, Container.Resolve<Migrations.Configuration>()));


            //run the discord bot
            if (config.DiscordBotEnabled)
            {
                _runDiscordBotCts = new CancellationTokenSource();
                _runDiscordBotTask = await Task.Factory.StartNew(async () => await RunDiscordBot(), _runDiscordBotCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            else Console.AddLog("Discord bot is disabled.");

            //server/cluster contexts
            if (config.Servers?.Length > 0)
            {
                foreach (var server in config.Servers)
                {
                    var context = new ArkServerContext(config, server, progress);
                    context.QueueManualUpdate();
                    _serverContexts.Add(server.Key, context);
                }
            }

            if (config.Clusters?.Length > 0)
            {
                foreach (var cluster in config.Clusters)
                {
                    var context = new ArkClusterContext(cluster);
                    _clusterContexts.Add(cluster.Key, context);
                }
            }

            //load the species stats data
            await ArkSpeciesStats.Instance.LoadOrUpdate();

            //webapi
            _webapi = WebApp.Start<Startup>(url: config.WebApiListenPrefix);
            Console.AddLog("WebAPI started");
        }

        private Task _runDiscordBotTask;
        private CancellationTokenSource _runDiscordBotCts;

        public async Task RunDiscordBot()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var config = scope.Resolve<IConfig>();
                var constants = scope.Resolve<IConstants>();
                var context = scope.Resolve<IArkContext>();
                var savedstate = scope.Resolve<ISavedState>();
                var skipExtract = savedstate.SkipExtractNextRestart;
                if (skipExtract)
                {
                    savedstate.SkipExtractNextRestart = false;
                    savedstate.Save();
                }


                //create database immediately to support direct (non-ef) access in application
                using (var db = scope.Resolve<IEfDatabaseContext>())
                {
                    var dir = Path.GetDirectoryName(constants.DatabaseFilePath);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    db.Database.Initialize(false);
                    //context.Database.Create();
                }

                var _bot = scope.Resolve<ArkDiscordBot>();
                await _bot.Initialize(_runDiscordBotCts.Token, skipExtract);
                var isConnected = false;
                var lastAttempt = DateTime.MinValue;
                var retryInterval = TimeSpan.FromSeconds(15);

                while (true)
                {
                    if (_runDiscordBotCts.IsCancellationRequested) break;

                    //if (Console.KeyAvailable)
                    //{
                    //    var key = Console.ReadKey(true);
                    //    if (key.Modifiers == ConsoleModifiers.Shift && key.Key == ConsoleKey.Enter) break;
                    //    else if (isConnected && key.Key == ConsoleKey.N && config.Debug)
                    //    {
                    //        //if we are debugging, trigger new changed event
                    //        context.DebugTriggerOnChange();
                    //    }
                    //}

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
                    _webapi?.Dispose();
                    _webapi = null;
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
