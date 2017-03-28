using ArkBot.Threading;
using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private Signaller<ContextUpdatingEventArgs> _signaller;

        private const int _saveWorldDefaultTimeoutSeconds = 180;

        public ArkServerService(IConstants constants, IConfig config, IArkContext context)
        {
            _signaller = new Signaller<ContextUpdatingEventArgs>();

            _constants = constants;
            _config = config;
            _context = context;

            _context.Updating += _context_Updating;
        }

        private void _context_Updating(object sender, ContextUpdatingEventArgs e)
        {
            if (e.WasTriggeredBySaveFileChange)
            {
                _signaller.PulseAll(e);
            }
        }

        public async Task<bool> SaveWorld(Func<string, Task<Message>> sendMessageDirected, int timeoutSeconds = _saveWorldDefaultTimeoutSeconds, bool noUpdateForThisCall = false)
        {
            if (timeoutSeconds <= 0) timeoutSeconds = _saveWorldDefaultTimeoutSeconds;

            if (sendMessageDirected != null) await sendMessageDirected($"saving world (may take a while)...");
            try
            {
                if (noUpdateForThisCall) _context.DisableContextUpdates();
                if (await CommandHelper.SendRconCommand(_config, "saveworld") == null)
                {
                    //failed to connect/exception/etc.
                    if (sendMessageDirected != null) await sendMessageDirected($"failed to save world.");
                    return false;
                }

                //wait for a save file change event
                ContextUpdatingEventArgs state = null;
                if (!_signaller.Wait(TimeSpan.FromSeconds(timeoutSeconds), out state))
                {
                    if (sendMessageDirected != null) await sendMessageDirected($"timeout while waiting for savegame write (save status unknown).");
                    return false;
                }
                if (_config.BackupsEnabled && !(state?.SavegameBackupResult?.ArchivePaths?.Length >= 1))
                {
                    if (sendMessageDirected != null) await sendMessageDirected($"savegame backup failed...");
                    return false;
                }
            }
            finally
            {
                if (noUpdateForThisCall) _context.EnableContextUpdates();
            }

            if (sendMessageDirected != null) await sendMessageDirected($"world saved!");
            return true;
        }

        public async Task<bool> ShutdownServer(Func<string, Task<Message>> sendMessageDirected, bool warnIfServerIsNotStarted = true)
        {
            var success = false;

            if (!Process.GetProcessesByName(_constants.ArkServerProcessName).Any())
            {
                if (warnIfServerIsNotStarted && sendMessageDirected != null) await sendMessageDirected($"the server is not started.");
                return !warnIfServerIsNotStarted;
            }

            //save world 
            if (!await SaveWorld(sendMessageDirected != null ? (s) => sendMessageDirected(s) : (Func<string, Task<Message>>)null, noUpdateForThisCall: true)) return false;

            if (sendMessageDirected != null) await sendMessageDirected($"shutting down the server...");
            var result2 = await CommandHelper.SendRconCommand(_config, "doexit");
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
            } while (Process.GetProcessesByName(_constants.ArkServerProcessName).Any());

            if (timeout)
            {
                if (sendMessageDirected != null) await sendMessageDirected($"timeout while waiting for the server to shutdown...");
                return success;
            }

            if (sendMessageDirected != null) await sendMessageDirected($"server shutdown complete!");
            success = true;

            return success;
        }

        public async Task<bool> RestartServer(Func<string, Task<Message>> sendMessageDirected)
        {
            if (!await ShutdownServer(sendMessageDirected != null ? (s) => sendMessageDirected(s) : (Func<string, Task<Message>>)null, false)) return false;
            if (!await StartServer(sendMessageDirected != null ? (s) => sendMessageDirected(s) : (Func<string, Task<Message>>)null)) return false;

            return true;
        }

        public async Task<bool> StartServer(Func<string, Task<Message>> sendMessageDirected, bool warnIfServerIsAlreadyStarted = true)
        {
            var success = false;

            if (Process.GetProcessesByName(_constants.ArkServerProcessName).Any())
            {
                if (warnIfServerIsAlreadyStarted && sendMessageDirected != null) await sendMessageDirected($"the server is already running.");
                return !warnIfServerIsAlreadyStarted;
            }

            var result = await ProcessHelper.RunCommandLineTool(_config.StartServerBatchFilePath, "");
            if (sendMessageDirected != null) await sendMessageDirected($"server is starting!");

            success = true;
            return success;
        }

        public async Task<bool> UpdateServer(Func<string, Task<Message>> sendMessageDirected, Func<string, string> getMessageDirected)
        {
            if (!await ShutdownServer(sendMessageDirected != null ? (s) => sendMessageDirected(s) : (Func<string, Task<Message>>)null, false)) return false;

            var updateMessage = sendMessageDirected != null ? await sendMessageDirected($"updating server...") : null;
            var r = new Regex(@"(?<task>\w+),\s+progress\:\s+(?<progress>[\d\.]+)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            var sb = new StringBuilder();
            var result = await ProcessHelper.RunCommandLineTool(_config.UpdateServerBatchFilePath, "", outputDataReceived: new Func<string, int>((s) =>
            {
                //Success! App '376030' already up to date.
                //Update state (0x11) preallocating, progress: 76.88 (3826221085 / 4976891598)
                //Update state (0x61) downloading, progress: 5.88 (292616661 / 4976891598)
                if (s != null) sb.AppendLine(s);
                if (s != null && updateMessage != null && getMessageDirected != null)
                {
                    var m = r.Match(s);
                    if (m.Success)
                    {
                        double d;
                        var p = "";
                        if (double.TryParse(m.Groups["progress"].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d)) p = $" {d:N1}%";
                        updateMessage.Edit(getMessageDirected?.Invoke($"updating server ({m.Groups["task"].Value}{p})..."));
                    }
                }

                return 0;
            }));
            if (result.ExitCode != 0)
            {
                if (sendMessageDirected != null) await sendMessageDirected($"failed to update server!");
                Logging.LogException(string.Join(Environment.NewLine, new[] { $@"Server update failed (exitCode: {result.ExitCode})", sb.ToString() }), result.Exception, GetType(), LogLevel.DEBUG);
                return false;
            }

            Logging.Log(string.Join(Environment.NewLine, new[] { $@"Server update successfull", sb.ToString() }), GetType(), LogLevel.DEBUG);
            if (sendMessageDirected != null) await sendMessageDirected($"update complete!");

            if (!await StartServer(sendMessageDirected != null ? (s) => sendMessageDirected(s) : (Func<string, Task<Message>>)null)) return false;

            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _context.Updating -= _context_Updating;
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
