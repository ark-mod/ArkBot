using ArkBot.Ark;
using ArkBot.Services.Data;
using ArkBot.Threading;
using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    /// <summary>
    /// Service for interacting with ARK Servers (Start/Stop/Save/Update/Restart etc.)
    /// </summary>
    public class ArkServerService : IArkServerService
    {
        private IConstants _constants;
        private IConfig _config;
        private IArkContext _context;
        private Signaller<Tuple<ArkServerContext, bool, SavegameBackupResult>> _signaller;
        private ArkContextManager _contextManager;

        private const int _saveWorldDefaultTimeoutSeconds = 180;
        private const int _updateDefaultTimeoutSeconds = 300;

        public ArkServerService(IConstants constants, IConfig config, IArkContext context, ArkContextManager contextManager)
        {
            _signaller = new Signaller<Tuple<ArkServerContext, bool, SavegameBackupResult>>();

            _constants = constants;
            _config = config;
            _context = context;
            _contextManager = contextManager;

            contextManager.BackupCompleted += ContextManager_BackupCompleted;
        }

        private void ContextManager_BackupCompleted(ArkServerContext sender, bool backupsEnabled, SavegameBackupResult result)
        {
            _signaller.PulseAll(Tuple.Create(sender, backupsEnabled, result));
        }

        public async Task<bool> SaveWorld(string serverKey, Func<string, Task<Message>> sendMessageDirected, int timeoutSeconds = _saveWorldDefaultTimeoutSeconds, bool noUpdateForThisCall = false)
        {
            //todo: it would be much better is SaveWorld considered if the save request ends up in a queue or not.

            if (timeoutSeconds <= 0) timeoutSeconds = _saveWorldDefaultTimeoutSeconds;

            var serverContext = _contextManager.GetServer(serverKey);
            if (serverContext == null)
            {
                if (sendMessageDirected != null) await sendMessageDirected($"could not find server instance {serverKey}");
                return false;
            }

            if (sendMessageDirected != null) await sendMessageDirected($"saving world (may take a while)...");
            try
            {
                if (noUpdateForThisCall) _context.DisableContextUpdates();
                if (await serverContext.Steam.SendRconCommand("saveworld") == null)
                {
                    //failed to connect/exception/etc.
                    if (sendMessageDirected != null) await sendMessageDirected($"failed to save world.");
                    return false;
                }

                //wait for a save file change event
                Tuple<ArkServerContext, bool, SavegameBackupResult> state = null;
                var now = DateTime.Now;
                while (true)
                {
                    var timeout = TimeSpan.FromSeconds(timeoutSeconds) - (DateTime.Now - now);
                    if (timeout.TotalSeconds <= 0 || !_signaller.Wait(timeout, out state))
                    {
                        if (sendMessageDirected != null) await sendMessageDirected($"timeout while waiting for savegame write (save status unknown).");
                        return false;
                    }
                    if (state?.Item1.Config.Key.Equals(serverKey, StringComparison.OrdinalIgnoreCase) != true)
                    {
                        continue;
                    }
                    if (_config.BackupsEnabled && !(state?.Item3?.ArchivePaths?.Length >= 1))
                    {
                        if (sendMessageDirected != null) await sendMessageDirected($"savegame backup failed...");
                        return false;
                    }

                    break;
                }
            }
            finally
            {
                if (noUpdateForThisCall) _context.EnableContextUpdates();
            }

            if (sendMessageDirected != null) await sendMessageDirected($"world saved!");
            return true;
        }

        public async Task<bool> ShutdownServer(string serverKey, Func<string, Task<Message>> sendMessageDirected, bool warnIfServerIsNotStarted = true)
        {
            var success = false;

            var serverContext = _contextManager.GetServer(serverKey);
            if (serverContext == null)
            {
                if (sendMessageDirected != null) await sendMessageDirected($"could not find server instance {serverKey}");
                return false;
            }

            var ids = GetProcessIdsForServerKey(serverKey);
            if (ids.Length <= 0)
            {
                if (warnIfServerIsNotStarted && sendMessageDirected != null) await sendMessageDirected($"could not find a running server with -serverkey={serverKey}");
                return !warnIfServerIsNotStarted;
            }
            else if (ids.Length > 1)
            {
                if (sendMessageDirected != null) await sendMessageDirected($"found multiple running servers with same -serverkey={serverKey}");
                return false;
            }
            var processId = ids.First();

            //save world 
            if (!await SaveWorld(serverKey, sendMessageDirected != null ? (s) => sendMessageDirected(s) : (Func<string, Task<Message>>)null, noUpdateForThisCall: true)) return false;

            if (sendMessageDirected != null) await sendMessageDirected($"shutting down the server...");
            var result2 = await serverContext.Steam.SendRconCommand("doexit");
            if (result2 == null)
            {
                if (sendMessageDirected != null) await sendMessageDirected($"failed to shutdown the server.");
                return success;
            }

            var n = 0;
            var timeout = false;
            do
            {
                if (n >= 60)
                {
                    timeout = true;
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
                n++;
            } while (ProcessHelper.IsProcessStarted((int)processId));

            if (timeout)
            {
                if (sendMessageDirected != null) await sendMessageDirected($"timeout while waiting for the server to shutdown...");
                return success;
            }

            if (sendMessageDirected != null) await sendMessageDirected($"server shutdown complete!");
            success = true;

            return success;
        }

        public async Task<bool> RestartServer(string serverKey, Func<string, Task<Message>> sendMessageDirected)
        {
            if (!await ShutdownServer(serverKey, sendMessageDirected != null ? (s) => sendMessageDirected(s) : (Func<string, Task<Message>>)null, false)) return false;
            if (!await StartServer(serverKey, sendMessageDirected != null ? (s) => sendMessageDirected(s) : (Func<string, Task<Message>>)null)) return false;

            return true;
        }

        public async Task<bool> StartServer(string serverKey, Func<string, Task<Message>> sendMessageDirected, bool warnIfServerIsAlreadyStarted = true)
        {
            var success = false;

            var serverContext = _contextManager.GetServer(serverKey);
            if (serverContext == null)
            {
                if (sendMessageDirected != null) await sendMessageDirected($"could not find server instance {serverKey}");
                return false;
            }

            var ids = GetProcessIdsForServerKey(serverKey);
            if (ids.Length > 0)
            {
                if (sendMessageDirected != null) await sendMessageDirected($"there is already one or more running servers with same -serverkey={serverKey}");
                return false;
            }

            if (sendMessageDirected != null) await sendMessageDirected($"starting the server...");

            //start ShooterGameServer "TheIsland?listen?Port=7006?QueryPort=27003?RCONEnabled=True?RCONPort=32333?SessionName=ARKSverige.se - PvE Lowrate [Cluster]?ServerAdminPassword=?SpectatorPassword=?AllowCrateSpawnsOnTopOfStructures=true?AllowAnyoneBabyImprintCuddle=true?ForceAllowCaveFlyers=True?PvEAllowStructuresAtSupplyDrops=True?PreventDownloadSurvivors=False?PreventDownloadItems=False?PreventDownloadDinos=False?PreventUploadSurvivors=False?PreventUploadItems=False?PreventUploadDinos=False"  -server -log -servergamelog -culture=en -StasisKeepControllers -NoTransferFromFiltering -servergamelogincludetribelogs -ClusterDirOverride=D:\servers\arksverige\pve -clusterid=pvelowrate
            try
            {
                var si = new ProcessStartInfo
                {
                    FileName = serverContext.Config.ServerExecutablePath,
                    Arguments = serverContext.Config.ServerExecutableArguments,
                    WorkingDirectory = Path.GetDirectoryName(serverContext.Config.ServerExecutablePath),
                    Verb = "runas",
                    UseShellExecute = false
                };
                Process.Start(si);
            }
            catch (Exception ex)
            {
                Logging.LogException($"Failed to start server instance ({serverKey})", ex, typeof(ArkServerService), LogLevel.ERROR, ExceptionLevel.Ignored);
                if (sendMessageDirected != null) await sendMessageDirected($"failed to start the server (check the configuration).");
                return false;
            }

            //var result = await ProcessHelper.RunCommandLineTool(serverContext.Config.StartBatchFilePath, "");
            if (sendMessageDirected != null)
            {
                var n = 0;
                var timeout = false;
                do
                {
                    if (n >= 10)
                    {
                        timeout = true;
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                    n++;
                } while (GetProcessIdsForServerKey(serverKey).Length <= 0);

                if (timeout) await sendMessageDirected($"failed to start the server."); //the script probably failed to start the process
                else await sendMessageDirected($"server is now booting up!");
            }

            success = true;
            return success;
        }

        public async Task<bool> UpdateServer(string serverKey, Func<string, Task<Message>> sendMessageDirected, Func<string, string> getMessageDirected, int timeoutSeconds = _updateDefaultTimeoutSeconds)
        {
            if (timeoutSeconds <= 0) timeoutSeconds = _updateDefaultTimeoutSeconds;

            var serverContext = _contextManager.GetServer(serverKey);
            if (serverContext == null)
            {
                if (sendMessageDirected != null) await sendMessageDirected($"could not find server instance {serverKey}");
                return false;
            }

            if (!await ShutdownServer(serverKey, sendMessageDirected != null ? (s) => sendMessageDirected(s) : (Func<string, Task<Message>>)null, false)) return false;

            var updateMessage = sendMessageDirected != null ? await sendMessageDirected($"updating server...") : null;
            //var r = new Regex(@"(?<task>\w+),\s+progress\:\s+(?<progress>[\d\.]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            //var sb = new StringBuilder();
            //var result = await ProcessHelper.RunCommandLineTool(serverContext.Config.UpdateBatchFilePath, "", keepOpen: true, hiddenWindow: false, outputDataReceived: new Func<string, int>((s) =>
            //{
            //    //Success! App '376030' already up to date.
            //    //Update state (0x11) preallocating, progress: 76.88 (3826221085 / 4976891598)
            //    //Update state (0x61) downloading, progress: 5.88 (292616661 / 4976891598)
            //    if (s != null) sb.AppendLine(s);
            //    if (s != null && updateMessage != null && getMessageDirected != null)
            //    {
            //        if (s?.StartsWith("Failed to load script file", StringComparison.OrdinalIgnoreCase) == true)
            //        {
            //            return -1;
            //        }

            //        var m = r.Match(s);
            //        if (m.Success)
            //        {
            //            double d;
            //            var p = "";
            //            if (double.TryParse(m.Groups["progress"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d)) p = $" {d:N1}%";
            //            updateMessage.Edit(getMessageDirected?.Invoke($"updating server ({m.Groups["task"].Value}{p})..."));
            //        }
            //    }

            //    return 0;
            //}));
            //if (result.ExitCode != 0)
            //{
            //    if (sendMessageDirected != null) await sendMessageDirected($"failed to update server!");
            //    Logging.LogException(string.Join(Environment.NewLine, new[] { $@"Server update failed (exitCode: {result.ExitCode})", sb.ToString() }), result.Exception, GetType(), LogLevel.DEBUG);
            //    return false;
            //}

            Timer timer = null;
            Process process = null;
            int result = -1;
            var sb = new StringBuilder();
            try
            {
                var wasAlreadyUpToDate = false;
                var wasUpdated = false;
                var error = false;
                string lastOutput = null;
                var lastUpdate = DateTime.Now;
                var r = new Regex(@"(?<task>\w+),\s+progress\:\s+(?<progress>[\d\.]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                var tmpPath = Path.GetTempFileName();
                File.WriteAllText(tmpPath, $"@ShutdownOnFailedCommand 1{Environment.NewLine}@NoPromptForPassword 1{Environment.NewLine}login anonymous{Environment.NewLine}force_install_dir \"{serverContext.Config.ServerInstallDirPath}\"{Environment.NewLine}app_update 376030{Environment.NewLine}quit");
                var si = new ProcessStartInfo
                {
                    FileName = serverContext.Config.SteamCmdExecutablePath,
                    Arguments = $@"+runscript {tmpPath}",
                    WorkingDirectory = Path.GetDirectoryName(serverContext.Config.SteamCmdExecutablePath),
                    Verb = "runas",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                var ss = new AutoResetEvent(false);
                var tcs = new TaskCompletionSource<int>();
                process = new Process
                {
                    StartInfo = si,
                    EnableRaisingEvents = true
                };
                process.OutputDataReceived += (s, e) =>
                {
                    //Success! App '376030' already up to date.
                    //Update state (0x11) preallocating, progress: 76.88 (3826221085 / 4976891598)
                    //Update state (0x61) downloading, progress: 5.88 (292616661 / 4976891598)
                    lastUpdate = DateTime.Now;
                    if (e?.Data != null) sb.AppendLine(e.Data);
                    if (e?.Data != null)
                    {
                        if (e.Data.StartsWith("Success! App '376030' already up to date.")) wasAlreadyUpToDate = true;
                        if (e.Data.StartsWith("Success! App '376030' fully installed.")) wasUpdated = true;
                        if (e.Data.StartsWith("Error! App '376030'")) error = true; //Error! App '376030' state is 0x202 after update job.
                        lastOutput = e.Data;
                        Debug.WriteLine(e.Data);
                    }
                };
                process.Exited += (sender, args) => tcs.TrySetResult(process.ExitCode);
                if (!process.Start())
                {
                    if (sendMessageDirected != null) await sendMessageDirected($"failed to update the server (check the configuration).");
                    return false;
                }
                process.BeginOutputReadLine();
                timer = new Timer((s) =>
                {
                    if ((DateTime.Now - lastUpdate).TotalSeconds >= timeoutSeconds) tcs.TrySetCanceled();
                    if (lastOutput != null && updateMessage != null && getMessageDirected != null)
                    {
                        string message = null;
                        if (wasAlreadyUpToDate) message = "already up to date";
                        else if (wasUpdated) message = "fully installed";
                        else if (error) message = "error";
                        else
                        {
                            var m = r.Match(lastOutput);
                            if (m.Success)
                            {
                                double d;
                                var p = "";
                                if (double.TryParse(m.Groups["progress"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d)) p = $" {d:N1}%";
                                message = $"{m.Groups["task"].Value}{p}";
                            }
                        }

                        if (message != null) updateMessage.Edit(getMessageDirected.Invoke($"updating server ({message})..."));
                    };
                }, null, 1000, 10000);

                result = await tcs.Task;
                timer.Change(0, Timeout.Infinite);
                if (result != 0 || error)
                {
                    Logging.Log(string.Join(Environment.NewLine, new[] { $@"Failed to update server instance ({serverKey}) [exitCode: {result}]", sb.ToString() }), typeof(ArkServerService), LogLevel.ERROR);
                    if (sendMessageDirected != null) await sendMessageDirected($"failed to update the server (check the configuration)");
                    return false;
                }

                Logging.Log(string.Join(Environment.NewLine, new[] { $@"Updated server instance ({serverKey})", sb.ToString() }), typeof(ArkServerService), LogLevel.INFO);

                if (sendMessageDirected != null) await sendMessageDirected($"update complete!");

                if (!await StartServer(serverKey, sendMessageDirected != null ? (s) => sendMessageDirected(s) : (Func<string, Task<Message>>)null)) return false;

                return true;
            }
            catch (OperationCanceledException)
            {
                Logging.Log(string.Join(Environment.NewLine, new[] { $@"Timeout while attempting to update server instance ({serverKey})", sb.ToString() }), typeof(ArkServerService), LogLevel.ERROR);
                if (sendMessageDirected != null) await sendMessageDirected($"timeout while waiting for the server update process to finish...");
                return false;
            }
            catch (Exception ex)
            {
                Logging.LogException(string.Join(Environment.NewLine, new[] { $@"Failed to update server instance ({serverKey}) [exitCode: {result}]", sb.ToString() }), ex, typeof(ArkServerService), LogLevel.ERROR, ExceptionLevel.Ignored);
                if (sendMessageDirected != null) await sendMessageDirected($"failed to update the server (check the configuration)");
                return false;
            }
            finally
            {
                timer?.Dispose();
                timer = null;
            }


            //Logging.Log(string.Join(Environment.NewLine, new[] { $@"Server update successfull", sb.ToString() }), GetType(), LogLevel.DEBUG);
            //if (sendMessageDirected != null) await sendMessageDirected($"update complete!");

            //if (!await StartServer(serverKey, sendMessageDirected != null ? (s) => sendMessageDirected(s) : (Func<string, Task<Message>>)null)) return false;

            //return true;
        }

        private uint[] GetProcessIdsForServerKey(string serverKey)
        {
            var r_serverkey = new Regex(@"-serverkey=(?<serverkey>[^\s]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            var info = GetProcessStartupInfo($"{_constants.ArkServerProcessName}.exe");
            var matchingProcess = info?.Where(x =>
            {
                var m = r_serverkey.Match(x.Item4);
                return m.Success ? m.Groups["serverkey"].Value.Equals(serverKey, StringComparison.OrdinalIgnoreCase) : false;
            }).ToArray();

            return matchingProcess?.Select(x => x.Item1).ToArray() ?? new uint[] { };
        }

        private static Tuple<uint, string, string, string>[] GetProcessStartupInfo(string processName)
        {
            var list = new List<Tuple<uint, string, string, string>>();
            try
            {
                //ProcessId, Name, ExecutablePath, CommandLine
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT ProcessId, Name, ExecutablePath, CommandLine FROM Win32_Process WHERE Name = '{processName}'"))
                {
                    using (ManagementObjectCollection moc = searcher.Get())
                    {
                        foreach(var mo in moc)
                        {
                            list.Add(Tuple.Create((uint)mo.GetPropertyValue("ProcessId"), (string)mo.GetPropertyValue("Name"), (string)mo.GetPropertyValue("ExecutablePath"), (string)mo.GetPropertyValue("CommandLine")));
                        }
                    }
                }
            }
            catch { /*ignore exception*/ }

            return list.ToArray();
        }
    }
}
