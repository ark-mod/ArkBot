using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Helpers
{
    public static class ProcessHelper
    {
        public class ProcessResult
        {
            public int ExitCode { get; set; }
            public Exception Exception { get; set; }
            //public string StandardOutput { get; set; }
            //public string StandardError { get; set; }
        }

        public static async Task<ProcessResult> RunCommandLineTool(string executableAbsolutePath, string commandLineArguments, string workingDirectory = null, bool hiddenWindow = true, Func<string, int> outputDataReceived = null, bool keepOpen = false)
        {
            int exitCode = 0;
            //string standardOutput = null;
            //string standardError = null;
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
                    //RedirectStandardError = redirectStandardError,
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
                if (outputDataReceived != null)  cmd.BeginOutputReadLine();

                ////blocking
                //standardOutput = cmd.StandardOutput.ReadToEnd();
                //standardError = cmd.StandardError.ReadToEnd();

                exitCode = await tcs.Task;
            }
            catch(Exception ex)
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
                //,StandardError = standardError,
                //StandardOutput = standardOutput
            };
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
