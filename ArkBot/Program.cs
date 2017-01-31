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
        static private ArkBot _bot;

        static private string saveFilePath => ConfigurationManager.AppSettings["saveFilePath"];
        static private string arktoolsExecutablePath => ConfigurationManager.AppSettings["arktoolsExecutablePath"];
        static private string jsonOutputDirPath => ConfigurationManager.AppSettings["jsonOutputDirPath"];
        static private string tempFileOutputDirPath => ConfigurationManager.AppSettings["tempFileOutputDirPath"];
        static private string botToken => ConfigurationManager.AppSettings["botToken"];

        static void Main(string[] args)
        {
            if (string.IsNullOrWhiteSpace(saveFilePath) || !File.Exists(saveFilePath))
                throw new ApplicationException($"AppSettings: {nameof(saveFilePath)} is not a valid filepath.");

            if (string.IsNullOrWhiteSpace(arktoolsExecutablePath) || !File.Exists(arktoolsExecutablePath))
                throw new ApplicationException($"AppSettings: {nameof(arktoolsExecutablePath)} is not a valid filepath.");

            if (string.IsNullOrWhiteSpace(jsonOutputDirPath) || !Directory.Exists(jsonOutputDirPath))
                throw new ApplicationException($"AppSettings: {nameof(jsonOutputDirPath)} is not a valid directory.");

            if (string.IsNullOrWhiteSpace(tempFileOutputDirPath) || !Directory.Exists(tempFileOutputDirPath))
                throw new ApplicationException($"AppSettings: {nameof(tempFileOutputDirPath)} is not a valid directory.");

            if (string.IsNullOrWhiteSpace(botToken))
                throw new ApplicationException($"AppSettings: {nameof(botToken)} is not set.");

            AsyncContext.Run(() => MainAsync());
        }

        static async Task MainAsync()
        {
            using (_bot = new ArkBot(saveFilePath, arktoolsExecutablePath, jsonOutputDirPath, tempFileOutputDirPath))
            {
                await _bot.Start(botToken);

                Console.ReadKey();
                await _bot.Stop();
            }
        }
    }
}
