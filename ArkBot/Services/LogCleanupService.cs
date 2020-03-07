using ArkBot.Helpers;
using QueryMaster.GameServer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Services
{
    public class LogCleanupService : ILogCleanupService
    {
        private Timer _timer;
        private readonly TimeSpan _delay = TimeSpan.FromHours(24);

        private const string _logDirectoryPath = @"logs\";
        private Regex _rDate = new Regex(@"_(?=\d{4,4})(?<date>.+?)(?:-\d{3,})?\.log", RegexOptions.IgnoreCase | RegexOptions.Singleline);


        public LogCleanupService()
        {
            _timer = new Timer(_timer_Callback, null, 0, Timeout.Infinite);
        }

        private void _timer_Callback(object state)
        {
            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);

                var t = DateTime.Now.AddDays(-30);

                foreach (var logFileName in Directory.GetFiles(_logDirectoryPath, "*.log", SearchOption.TopDirectoryOnly))
                {
                    var m = _rDate.Match(logFileName);
                    var dateStr = m.Success ? m.Groups["date"]?.Value : null;
                    if (dateStr == null) continue;

                    if (!DateTime.TryParseExact(dateStr, @"yyyy-MM-dd.HH.mm.ss.ffff", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var dt) || dt > t) continue;

                    File.Delete(logFileName);
                }
            }
            finally
            {
                _timer.Change(_delay, _delay);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _timer?.Dispose();
                    _timer = null;
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
