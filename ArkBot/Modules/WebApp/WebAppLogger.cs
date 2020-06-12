using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ArkBot.Modules.WebApp
{
    public static class WebAppLoggerFactoryExtensions
    {
        public static ILoggingBuilder AddWebApp(this ILoggingBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, WebAppLoggerProvider>());

            return builder;
        }
    }

    [ProviderAlias("WebApp")]
    public sealed class WebAppLoggerProvider : ILoggerProvider
    {
        private readonly Application.Configuration.Model.IConfig _config;

        public WebAppLoggerProvider(Application.Configuration.Model.IConfig config)
        {
            _config = config;
        }

        public ILogger CreateLogger(string name)
        {
            return new WebAppLogger(name, _config);
        }

        public void Dispose() { }
    }

    public sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();

        private NullScope() { }

        public void Dispose() { }
    }

    internal class WebAppLogger : ILogger
    {
        private readonly Application.Configuration.Model.IConfig _config;
        private readonly string _name;

        private static Dictionary<LogLevel, ArkBot.Utils.LogLevel> _logLevels;

        static WebAppLogger()
        {
            _logLevels = new Dictionary<LogLevel, Utils.LogLevel>
            {
                { LogLevel.Information, Utils.LogLevel.INFO },
                { LogLevel.Warning, Utils.LogLevel.WARN },
                { LogLevel.Error, Utils.LogLevel.ERROR },
                { LogLevel.Critical, Utils.LogLevel.FATAL },
                { LogLevel.Debug, Utils.LogLevel.DEBUG },
                { LogLevel.Trace, Utils.LogLevel.DEBUG }
            };
        }

        public WebAppLogger(string name, Application.Configuration.Model.IConfig config)
        {
            _name = name;
            _config = config;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            if (logLevel == LogLevel.None) return false;

            return logLevel >= (_config?.LogLevel ?? LogLevel.Warning);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (exception != null)
            {
                if (!_logLevels.TryGetValue(logLevel, out var internalLogLevel)) internalLogLevel = Utils.LogLevel.INFO;

                Application.ViewModel.Workspace.Instance.Console.AddLog(@$"{message} (""{exception.Message}"")", System.Windows.Media.Brushes.Red);
                Utils.Logging.LogException(message, exception, GetType(), internalLogLevel);
            }
            else
            {
                if (logLevel == LogLevel.Warning) Application.ViewModel.Workspace.Instance.Console.AddLogWarning(message);
                else if (logLevel >= LogLevel.Error) Application.ViewModel.Workspace.Instance.Console.AddLogError(message);
                else Application.ViewModel.Workspace.Instance.Console.AddLog(message);
            }
        }
    }
}