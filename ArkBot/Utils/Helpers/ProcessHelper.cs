using ArkBot.Modules.Application.Configuration.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Utils.Helpers
{
    public static class ProcessHelper
    {
        public class ProcessResult
        {
            public int ExitCode { get; set; }
            public Exception Exception { get; set; }
        }

        public static async Task<ProcessResult> RunCommandLineTool(string executableAbsolutePath, string commandLineArguments, string workingDirectory = null, bool hiddenWindow = true, Func<string, int> outputDataReceived = null, bool keepOpen = false)
        {
            int exitCode = 0;
            Exception exception = null;
            Process cmd = null;
            try
            {
                // /C     Run Command and then terminate
                // /K     Run Command and then return to the CMD prompt.

                var tcs = new TaskCompletionSource<int>();
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Verb = "runas",
                    Arguments = $@"{(keepOpen ? "\\K" : "\\C")} ""{executableAbsolutePath}"" {commandLineArguments}",
                    WorkingDirectory = workingDirectory ?? Directory.GetParent(executableAbsolutePath).FullName,
                    CreateNoWindow = hiddenWindow,
                    UseShellExecute = false,
                    WindowStyle = hiddenWindow ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                    RedirectStandardOutput = outputDataReceived != null
                };
                cmd = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };
                cmd.Exited += (sender, args) =>
                {
                    if (cmd.ExitCode != 0) tcs.SetResult(cmd.ExitCode);
                };

                if (outputDataReceived != null)
                {
                    cmd.OutputDataReceived += (s, e) =>
                    {
                        var result = outputDataReceived(e.Data);
                        if (result != 0)
                        {
                            tcs.SetResult(result);
                        }
                    };
                }

                cmd.Start();
                if (outputDataReceived != null) cmd.BeginOutputReadLine();

                exitCode = await tcs.Task;
            }
            catch (Exception ex)
            {
                exitCode = int.MinValue;
                exception = ex;
            }
            finally
            {
                cmd?.Dispose();
                cmd = null;
            }

            return new ProcessResult
            {
                ExitCode = exitCode,
                Exception = exception
            };
        }

        public static async Task<(bool success, string output)> RunCommandLine(string command, IConfig config, Action<string> onOutputLineRead = null, bool UsePowershellOutputRedirect = true, int timeoutSeconds = 10)
        {
            Timer timer = null;
            Process process = null;
            FileStream powershellOutputStream = null;
            StreamReader powershellOutputStreamReader = null;
            string tmpFilePathToPowershellOutput = null;
            int result = -1;
            var sb = new StringBuilder();
            try
            {
                var lastUpdate = DateTime.Now;
                var si = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    Verb = "runas",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };

                if (UsePowershellOutputRedirect)
                {
                    var debugNoExit = false;
                    tmpFilePathToPowershellOutput = Path.GetTempFileName();
                    si = new ProcessStartInfo
                    {
                        FileName = config.PowershellFilePath,
                        Arguments = $@"{(debugNoExit ? "-NoExit " : "")}& {command} | tee {tmpFilePathToPowershellOutput}",
                        Verb = "runas",
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                }
                var ss = new AutoResetEvent(false);
                var tcs = new TaskCompletionSource<int>();
                process = new Process
                {
                    StartInfo = si,
                    EnableRaisingEvents = true
                };
                if (!UsePowershellOutputRedirect)
                {
                    process.OutputDataReceived += (s, e) =>
                    {
                        lastUpdate = DateTime.Now;
                        if (e?.Data != null)
                        {
                            sb.AppendLine(e.Data);
                            onOutputLineRead?.Invoke(e.Data);
                        }
                    };
                }
                else
                {
                    var offset = 0L;
                    powershellOutputStream = new FileStream(tmpFilePathToPowershellOutput, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    powershellOutputStreamReader = new StreamReader(powershellOutputStream);
                    var task = Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (tcs.Task.IsCompleted) return;

                            powershellOutputStream.Seek(offset, SeekOrigin.Begin);
                            if (!powershellOutputStreamReader.EndOfStream)
                            {
                                do
                                {
                                    var line = powershellOutputStreamReader.ReadLine();

                                    if (line == null) continue;

                                    sb.AppendLine(line);
                                    onOutputLineRead?.Invoke(line);
                                } while (!powershellOutputStreamReader.EndOfStream);

                                offset = powershellOutputStream.Position;
                            }
                            else await Task.Delay(100);
                        }
                    });
                }
                process.Exited += (sender, args) => tcs.TrySetResult(process.ExitCode);
                if (!process.Start())
                {
                    return (false, sb.ToString());
                }
                if (!UsePowershellOutputRedirect) process.BeginOutputReadLine();
                timer = new Timer(async (s) =>
                {
                    if ((DateTime.Now - lastUpdate).TotalSeconds >= timeoutSeconds) tcs.TrySetCanceled();
                }, null, 1000, 10000);

                result = await tcs.Task;
                timer.Change(0, Timeout.Infinite);
                if (result != 0)
                {
                    return (false, sb.ToString());
                }

                return (true, sb.ToString());
            }
            catch (OperationCanceledException)
            {
                return (false, sb.ToString());
            }
            catch (Exception ex)
            {
                return (false, sb.ToString());
            }
            finally
            {
                powershellOutputStream?.Dispose();
                powershellOutputStream = null;
                powershellOutputStreamReader?.Dispose();
                powershellOutputStreamReader = null;
                timer?.Dispose();
                timer = null;

                try
                {
                    if (tmpFilePathToPowershellOutput != null) File.Delete(tmpFilePathToPowershellOutput);
                }
                catch { }
            }
        }

        public static bool IsProcessStarted(int processId)
        {
            try
            {
                return Process.GetProcessById(processId) != null;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
