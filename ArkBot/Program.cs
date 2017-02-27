using ArkBot.Data;
using ArkBot.Helpers;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RazorEngine;
using RazorEngine.Templating;
using System.Windows.Forms;
using Autofac;
using System.Reflection;
using ArkBot.Commands;
using ArkBot.OpenID;
using RazorEngine.Configuration;
using ArkBot.Services;
using ArkBot.Database;

namespace ArkBot
{
    class Program
    {
        private static IContainer Container { get; set; }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception) ExceptionLogging.LogUnhandledException(e.ExceptionObject as Exception, true);
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            ExceptionLogging.LogUnhandledException(e.Exception, true);
        }

        static void Main(string[] args)
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Console.WriteLine("ARK Discord Bot");
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine();

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
            catch(Exception ex)
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
            //todo: for now this section is not really needed unless !imprintcheck is used
            //if (config.ArkMultipliers == null)
            //{
            //    sb.AppendLine($@"Error: {nameof(config.ArkMultipliers)} section is missing from config file.");
            //    sb.AppendLine($@"Expected value: {ValidationHelper.GetDescriptionForMember(config, nameof(config.ArkMultipliers))}");
            //    sb.AppendLine();
            //}

            //load aliases and check integrity
            var aliases = ArkSpeciesAliases.Load().GetAwaiter().GetResult();
            if (aliases == null || !aliases.CheckIntegrity)
            {
                sb.AppendLine($@"Error: ""{ArkSpeciesAliases._filepath}"" is missing, contains invalid json or duplicate aliases.");
                if(aliases != null)
                {
                    foreach(var duplicateAlias in aliases.Aliases?.SelectMany(x => x).GroupBy(x => x)
                             .Where(g => g.Count() > 1)
                             .Select(g => g.Key))
                    {
                        sb.AppendLine($@"Duplicate alias: ""{duplicateAlias}""");
                    }
                }
                sb.AppendLine();
            }

            var errors = sb.ToString();
            if(errors.Length > 0)
            {
                WriteAndWaitForKey(errors);
                return;
            }

            IProgress<string> progress = new Progress<string>(message =>
            {
                Console.WriteLine(message);
            });

            var constants = new Constants();
            //var context = new ArkContext(config, constants, progress);

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
            builder.RegisterInstance(config).As<IConfig>();
            builder.RegisterType<ArkContext>().As<IArkContext>().WithParameter(new TypedParameter(typeof(IProgress<string>), progress)).SingleInstance();
            builder.RegisterAssemblyTypes(thisAssembly).As<ICommand>().SingleInstance()
                .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);
            builder.RegisterInstance(openId).As<IBarebonesSteamOpenId>();
            builder.RegisterType<EfDatabaseContext>().As<IEfDatabaseContext>();
            builder.RegisterType<DatabaseContextFactory<IEfDatabaseContext>>();
            builder.RegisterType<Migrations.Configuration>().PropertiesAutowired();

            Container = builder.Build();

            //update database
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.MigrateDatabaseToLatestVersion<EfDatabaseContext, Migrations.Configuration>(false, Container.Resolve<Migrations.Configuration>()));

            AsyncContext.Run(() => MainAsync());
        }

        static void WriteAndWaitForKey(params string[] msgs)
        {
            foreach(var msg in msgs) if(msg != null) Console.WriteLine(msg);
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task MainAsync()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                //create database immediately to support direct (non-ef) access in application
                using (var context = scope.Resolve<IEfDatabaseContext>())
                {
                    var constants = scope.Resolve<IConstants>();
                    if (!Directory.Exists(constants.DatabaseDirectoryPath)) Directory.CreateDirectory(constants.DatabaseDirectoryPath);

                    context.Database.Initialize(false);
                    //context.Database.Create();
                }

                var _bot = scope.Resolve<ArkDiscordBot>();
                await _bot.Start();

                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Modifiers == ConsoleModifiers.Shift && key.Key == ConsoleKey.Enter) break;
                    }

                    await Task.Delay(100);
                }

                await _bot.Stop();
            }
        }
    }
}
