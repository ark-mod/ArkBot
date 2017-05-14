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
using Discord;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
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
using System.Security.Principal;
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

        public ICommand SteamCmdTestCommand => _steamCmdTestCommand ?? (_steamCmdTestCommand = new RelayCommand(parameter => OnSteamCmdTest(parameter), parameter => CanSteamCmdTest(parameter)));
        private RelayCommand _steamCmdTestCommand;

        public DelegateCommand<System.ComponentModel.CancelEventArgs> ClosingCommand { get; private set; }

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

        private ArkContextManager _contextManager;
        private IConfig _config;

        public Workspace()
        {
            //do not create viewmodels or load data here, or avalondock layout deserialization will fail
            ManuallyUpdateServers = new ObservableCollection<MenuItemViewModel>();
            ManuallyUpdateClusters = new ObservableCollection<MenuItemViewModel>();
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

        private bool CanSteamCmdTest(object parameter)
        {
            return true;
        }

        private async void OnSteamCmdTest(object parameter)
        {
            if (MessageBox.Show("Are you sure you want to run a SteamCmd test?", "SteamCmd test", MessageBoxButton.YesNo, MessageBoxImage.None) != MessageBoxResult.Yes) return;
            var hidden = MessageBox.Show("As hidden window?", "SteamCmd test", MessageBoxButton.YesNo, MessageBoxImage.None);
            var nowindow = MessageBox.Show("Create no window?", "SteamCmd test", MessageBoxButton.YesNo, MessageBoxImage.None);
            var outputdata = MessageBox.Show("Redirect output?", "SteamCmd test", MessageBoxButton.YesNo, MessageBoxImage.None);
            var exitwhendone = MessageBox.Show("Exit steamcmd when script finishes?", "SteamCmd test", MessageBoxButton.YesNo, MessageBoxImage.None);

            var showMsg = new Action<string>((msg) => MessageBox.Show(msg, "SteamCmd test", MessageBoxButton.OK, MessageBoxImage.None));
            var serverContext = _contextManager.GetServer(_config.ServerKey);
            if (serverContext == null)
            {
                showMsg("Failed to get server instance.");
                return;
            }

            var n = 0;
            string tmpInstallDir = null;
            var tmpPath = Path.GetTempFileName();
            do
            {
                if (n >= 100)
                {
                    showMsg("Failed to create a temp directory.");
                    return;
                }
                tmpInstallDir = Path.Combine(_config.TempFileOutputDirPath, Guid.NewGuid().ToString());
                n++;
            } while (Directory.Exists(tmpInstallDir));

            try
            {
                Directory.CreateDirectory(tmpInstallDir);

                var exit = exitwhendone == MessageBoxResult.Yes ? $"{Environment.NewLine}quit" : "";
                File.WriteAllText(tmpPath, $"@ShutdownOnFailedCommand 1{Environment.NewLine}@NoPromptForPassword 1{Environment.NewLine}login anonymous{Environment.NewLine}force_install_dir \"{tmpInstallDir}\"{Environment.NewLine}app_update 376030{exit}");

                var tcs = new TaskCompletionSource<int>();
                var si = new ProcessStartInfo
                {
                    FileName = serverContext.Config.SteamCmdExecutablePath,
                    Arguments = $@"+runscript {tmpPath}",
                    WorkingDirectory = Path.GetDirectoryName(serverContext.Config.SteamCmdExecutablePath),
                    Verb = "runas",
                    UseShellExecute = false,
                    RedirectStandardOutput = outputdata == MessageBoxResult.Yes ? true : false,
                    WindowStyle = hidden == MessageBoxResult.Yes ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                    CreateNoWindow = nowindow == MessageBoxResult.Yes ? true : false
                };
                var process = new Process
                {
                    StartInfo = si,
                    EnableRaisingEvents = true
                };
                if (outputdata == MessageBoxResult.Yes)
                {
                    process.OutputDataReceived += (s, e) =>
                    {
                        if (e?.Data == null) return;
                        Console.AddLog(e.Data);
                    };
                }

                process.Exited += (sender, args) => tcs.TrySetResult(process.ExitCode);
                if (!process.Start())
                {
                    showMsg("Process failed to start");
                    return;
                }
                if(outputdata == MessageBoxResult.Yes) process.BeginOutputReadLine();

                if (nowindow == MessageBoxResult.Yes || hidden == MessageBoxResult.Yes)
                {
                    var closeTask = Task.Run(() =>
                    {
                        MessageBox.Show("Stop the test?", "SteamCmd test", MessageBoxButton.OK, MessageBoxImage.None);
                        process.Kill();
                    }).ConfigureAwait(false);
                }

                var result = await tcs.Task;
                showMsg($"Process exited with code {result}");
            }
            catch(Exception ex)
            {
                showMsg($"Exception: {ex.ToString()}");
            }
            finally
            {
                try
                {
                    if (tmpInstallDir.StartsWith(_config.TempFileOutputDirPath))
                    {
                        foreach (var file in Directory.GetFiles(tmpInstallDir))
                        {
                            File.Delete(file);
                        }
                        foreach (var dir in Directory.GetDirectories(tmpInstallDir))
                        {
                            Directory.Delete(dir, true);
                        }
                        Directory.Delete(tmpInstallDir);
                    }
                }
                catch (Exception) { }
            }
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

            log4net.Config.XmlConfigurator.Configure();

            //load config and check for errors
            var configPath = @"config.json";
            if (!File.Exists(configPath))
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
                _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
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
            if (string.IsNullOrWhiteSpace(_config.SaveFilePath) || !File.Exists(_config.SaveFilePath))
            {
                sb.AppendLine($@"Error: {nameof(_config.SaveFilePath)} is not a valid file path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.SaveFilePath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.ClusterSavePath) || !Directory.Exists(_config.ClusterSavePath))
            {
                sb.AppendLine($@"Error: {nameof(_config.ClusterSavePath)} is not a valid directory path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.ClusterSavePath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.ArktoolsExecutablePath) || !File.Exists(_config.ArktoolsExecutablePath))
            {
                sb.AppendLine($@"Error: {nameof(_config.ArktoolsExecutablePath)} is not a valid file path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.ArktoolsExecutablePath))}");
                sb.AppendLine();
            }
            if (string.IsNullOrWhiteSpace(_config.JsonOutputDirPath) || !Directory.Exists(_config.JsonOutputDirPath))
            {
                sb.AppendLine($@"Error: {nameof(_config.JsonOutputDirPath)} is not a valid directory path.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.JsonOutputDirPath))}");
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
            if (string.IsNullOrWhiteSpace(_config.ServerIp))
            {
                sb.AppendLine($@"Error: {nameof(_config.ServerIp)} is not set.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.ServerIp))}");
                sb.AppendLine();
            }
            if (_config.ServerPort <= 0)
            {
                sb.AppendLine($@"Error: {nameof(_config.ServerPort)} is not valid.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.ServerPort))}");
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
            if (serverkeys?.Length > 0 && (string.IsNullOrEmpty(_config.ServerKey) || !serverkeys.Contains(_config.ServerKey, StringComparer.OrdinalIgnoreCase)))
            {
                sb.AppendLine($@"Error: {nameof(_config.ServerKey)} must be set to a value server key if server instances are configured.");
                sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(_config, nameof(_config.ServerKey))}");
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

            var playedTimeWatcher = new PlayedTimeWatcher(_config);

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

            var discord = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Warning;
                x.LogHandler += (s, e) => Console.AddLog(e.Message);
                x.AppName = _config.BotName;
                x.AppUrl = !string.IsNullOrWhiteSpace(_config.BotUrl) ? _config.BotUrl : null;
            });

            //setup dependency injection
            var thisAssembly = Assembly.GetExecutingAssembly();
            var builder = new ContainerBuilder();

            builder.RegisterType<ArkServerContext>().AsSelf();
            if (_config.UseCompatibilityChangeWatcher) builder.RegisterType<ArkSaveFileWatcherTimer>().As<IArkSaveFileWatcher>();
            else builder.RegisterType<ArkSaveFileWatcher>().As<IArkSaveFileWatcher>();
            builder.RegisterInstance(discord).AsSelf();
            builder.RegisterType<ArkDiscordBot>();
            builder.RegisterType<UrlShortenerService>().As<IUrlShortenerService>().SingleInstance();
            builder.RegisterInstance(constants).As<IConstants>();
            builder.RegisterInstance(_savedstate).As<ISavedState>();
            builder.RegisterInstance(_config as Config).As<IConfig>();
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
            builder.RegisterHubs(Assembly.GetExecutingAssembly());
            builder.RegisterType<ArkContextManager>().WithParameter(new TypedParameter(typeof(IProgress<string>), progress)).AsSelf().SingleInstance();
            builder.RegisterType<VotingManager>().WithParameter(new TypedParameter(typeof(IProgress<string>), progress)).AsSelf().SingleInstance();
            builder.RegisterType<DiscordManager>().AsSelf().SingleInstance();
            builder.RegisterType<ScheduledTasksManager>().AsSelf().PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies).SingleInstance();
            builder.RegisterType<NotificationManager>().AsSelf().SingleInstance();

            builder.RegisterType<AutofacDependencyResolver>().As<IDependencyResolver>().SingleInstance();

            //kernel.Bind(typeof(IHubConnectionContext<dynamic>)).ToMethod(context =>
            //        resolver.Resolve<IConnectionManager>().GetHubContext<StockTickerHub>().Clients
            //         ).WhenInjectedInto<IStockTicker>();

            Container = builder.Build();

            //update database
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<EfDatabaseContext, Migrations.Configuration>(true, Container.Resolve<Migrations.Configuration>()));

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
                var backupService = Container.Resolve<ISavegameBackupService>();
                foreach (var server in _config.Servers)
                {
                    var context = Container.Resolve<ArkServerContext>(new TypedParameter(typeof(ServerConfigSection), server));
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

            //webapi
            _webapi = WebApp.Start<Startup>(url: _config.WebApiListenPrefix);
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
