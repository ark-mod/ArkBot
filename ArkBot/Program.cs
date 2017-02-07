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
using System.Threading.Tasks;

namespace ArkBot
{
    class Program
    {
        static private ArkDiscordBot _bot;
        static private Config _config;

        static void Main(string[] args)
        {
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

            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
            if (config == null)
            {
                WriteAndWaitForKey($@"The required file config.json is empty or contains errors. Please copy defaultconfig.json, set the correct values for your environment and restart the application.");
                return;
            }

            var sb = new StringBuilder();
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
