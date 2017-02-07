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

namespace ArkBot
{
    class Program
    {
        static private ArkDiscordBot _bot;
        static private Config _config;

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

            var WriteAndWaitForKey = new Action<string>((msg) =>
            {
                Console.WriteLine(msg);
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            });

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
            try
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
            }
            catch { /* ignore exceptions */ }
            if (config == null)
            {
                WriteAndWaitForKey($@"The required file config.json is empty or contains errors. Please copy defaultconfig.json, set the correct values for your environment and restart the application.");
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

            _config = config;

            AsyncContext.Run(() => MainAsync());
        }

        static async Task MainAsync()
        {
            IProgress<string> progress = new Progress<string>(message =>
            {
                Console.WriteLine(message);
            });
            using (_bot = new ArkDiscordBot(_config, progress))
            {
                await _bot.Start(_config.BotToken);

                while(true)
                {
                    if(Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                        break;
                    }

                    await Task.Delay(100);
                }

                await _bot.Stop();
            }
        }
    }
}
