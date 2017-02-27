using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Reflection;
using System.Collections;

namespace ArkBot
{
    public class ExceptionLogging
    {
        public static void LogUnhandledException(Exception exception, bool isCrash = false, string message = null, string source = null)
        {
            var filename = string.Format(@"{0}_{1}", (isCrash ? "applicationcrash" + (source != null ? "_" + source : "") : "exception" + (source != null ? "_" + source : "")), DateTime.Now.ToString("yyyy-MM-dd.HH.mm.ss.ffff"));
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
                    XElement root = new XElement("Exception");
                    root.Add(GetXElementExeception(exception));
                    sw.Write(root);
                }
            }
        }

        /// <summary>
        /// Log a non-fatal handled exception to file in order to keep track of runtime events.
        /// </summary>
        public static void LogException(Exception exception, string message = null, string source = null)
        {
            if (exception == null) return;
            LogUnhandledException(exception, false, message, source);
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
            xe.Add(new XElement("TargetSite", new XText(ex.TargetSite.ToString())));
            xe.Add(new XElement("Source", new XText(ex.Source ?? "null")));
            xe.Add(new XElement("StackTrace",
                new XCData(
                    string.Join(
                        Environment.NewLine,
                        (from st in ex.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList() let _st = st.Trim() select _st).ToArray()
                    )
                )
            ));
            if (ex.HelpLink != null) xe.Add(new XElement("HelpLink", new XText(ex.HelpLink ?? "null")));
            if (ex.Data.Count == 0) ; //xe.Add(new XElement("Data", "null"));
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
