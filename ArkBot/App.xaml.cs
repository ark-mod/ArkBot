using ArkBot.Interop;
using ArkBot.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;

namespace ArkBot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Console.SetOut(new WpfConsoleWriter());

            Kernel32.RegisterApplicationRestart("/restart", (int)RestartRestrictions.None);

            var settings = new CefSettings()
            {
                //LogSeverity = LogSeverity.Verbose,
                LogFile = "logs\\cefsharp.log",
                BrowserSubprocessPath = "lib\\CefSharp.BrowserSubprocess.exe",
            };

            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null) Logging.LogException(exception.Message, exception, GetType(), LogLevel.FATAL, ExceptionLevel.ApplicationCrash);
        }

        public class WpfConsoleWriter : TextWriter
        {
            private StringBuilder _sb = new StringBuilder();

            public override void Write(char value)
            {
                if (value == '\n')
                {
                    AddLineToConsole(_sb.ToString());
                    _sb.Clear();
                }
                else _sb.Append(value);
            }

            public override void Write(string value)
            {
                //var foreground = Console.ForegroundColor;
                //var background = Console.BackgroundColor;

                var index = value.IndexOf('\n');
                if (index == -1) _sb.Append(value);
                else
                {
                    if (index > 0) _sb.Append(value.Substring(0, index));
                    AddLineToConsole(_sb.ToString());
                    _sb.Clear();

                    if (index < value.Length - 1)
                    {
                        Write(value.Substring(index));
                    }
                }
            }

            private void AddLineToConsole(string line)
            {
                Workspace.Instance.Console.AddLog(line.TrimEnd('\n', '\r'));
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }
    }
}
