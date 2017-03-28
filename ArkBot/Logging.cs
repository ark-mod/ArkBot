using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;
using log4net;
using System.Text.RegularExpressions;

namespace ArkBot
{
    public enum LogLevel { DEBUG, INFO, WARN, ERROR, FATAL }
    public enum ExceptionLevel { ApplicationCrash, Unhandled, Ignored }

    public class Logging
    {
        private static readonly Dictionary<LogLevel, log4net.Core.Level> _levels = new Dictionary<LogLevel, log4net.Core.Level>
        {
            { LogLevel.DEBUG, log4net.Core.Level.Debug },
            { LogLevel.INFO, log4net.Core.Level.Info },
            { LogLevel.WARN, log4net.Core.Level.Warn },
            { LogLevel.ERROR, log4net.Core.Level.Error },
            { LogLevel.FATAL, log4net.Core.Level.Fatal },
        };
        
        public static void Log(string message, Type type, LogLevel level = LogLevel.INFO,
            [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogInternal(message, type, level: level, memberName: memberName, sourceFilePath: sourceFilePath, sourceLineNumber: sourceLineNumber);
        }

        public static void LogException(string message, Exception exception, Type type, LogLevel level = LogLevel.INFO, ExceptionLevel exceptionLevel = ExceptionLevel.Ignored,
            [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogInternal(message, type, exception, level, exceptionLevel, memberName, sourceFilePath, sourceLineNumber);
        }

        private static void LogInternal(string message, Type type, Exception exception = null, LogLevel level = LogLevel.INFO, ExceptionLevel exceptionLevel = ExceptionLevel.Ignored,
            string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
        {
            var log = LogManager.GetLogger(type);
            log.Logger.Log(typeof(Logging), _levels[level], message, exception);
            if (exception != null) LogXmlSerializedException(exception, exceptionLevel, message, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Log full exception data to file by serializing to xml
        /// </summary>
        public static void LogXmlSerializedException(Exception exception, ExceptionLevel level, string message = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
        {
            var location = $"[{memberName}({sourceFilePath}:{sourceLineNumber})]";
            var name = "exception";
            switch(level)
            {
                case ExceptionLevel.ApplicationCrash:
                    name = "applicationcrash";
                    break;
                case ExceptionLevel.Unhandled:
                    name = "unhandledexception";
                    break;
                case ExceptionLevel.Ignored:
                    name = "ignoredexception";
                    break;
            }

            var r = new Regex(@"[^a-z0-9\._]+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var filename = $@"logs\{name}{(!string.IsNullOrWhiteSpace(memberName) ? $"_{r.Replace(memberName, "_")}" : "")}_{DateTime.Now:yyyy-MM-dd.HH.mm.ss.ffff}";
            var ext = ".log";
            string path = null;
            var n = 0;
            while (true)
            {
                path = string.Format(@"{0}{1}{2}", filename, (n > 0 ? "-" + n.ToString("000") : ""), ext);
                if (!File.Exists(path)) break;
                else n++;
            }
            using (var ifs = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                using (var sw = new StreamWriter(ifs))
                {
                    if (message != null) sw.WriteLine(message);
                    sw.WriteLine(location);
                    sw.WriteLine();
                    XElement root = new XElement("Exception");
                    root.Add(GetXElementExeception(exception));
                    sw.Write(root);
                }
            }
        }

        protected static XElement GetXElementExeception(Exception ex)
        {
            var xe = new XElement(ex.GetType().FullName);

            //get non-inherited public properties for the current exception unless it is of base exception type (in-case we already have handled them)
            if (ex.GetType() != typeof(Exception))
            {
                var cxe = new XElement("Current");
                var props = ex.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.DeclaredOnly).ToList();
                foreach (var x in props)
                {
                    var val = x.GetValue(ex, null);
                    var sval = (val != null ? val.ToString() : "null");
                    cxe.Add(new XElement(x.Name, new XText(sval)));
                }
                xe.Add(cxe);
            }

            xe.Add(new XElement("Message", new XText(ex.Message ?? "null")));
            if(ex.TargetSite != null) xe.Add(new XElement("TargetSite", new XText(ex.TargetSite.ToString())));
            xe.Add(new XElement("Source", new XText(ex.Source ?? "null")));
            if (ex.StackTrace != null)
            {
                xe.Add(new XElement("StackTrace",
                    new XCData(
                        string.Join(
                            Environment.NewLine,
                            (from st in ex.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList() let _st = st.Trim() select _st).ToArray()
                        )
                    )
                ));
            }
            if (ex.HelpLink != null) xe.Add(new XElement("HelpLink", new XText(ex.HelpLink ?? "null")));
            if (ex.Data == null || ex.Data.Count == 0) ; //xe.Add(new XElement("Data", "null"));
            else xe.Add(new XElement("Data", from entry in ex.Data.Cast<DictionaryEntry>() let key = entry.Key.ToString() let value = (entry.Value != null ? entry.Value.ToString() : "null") select new XElement(key, value)));

            if (ex.InnerException != null)
            {
                var ixe = new XElement("InnerException");
                ixe.Add(GetXElementExeception(ex.InnerException));
                xe.Add(ixe);
            }

            return xe;
        }
    }
}
