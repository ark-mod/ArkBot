using ArkBot.Ark;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot
{
    public class ArkSaveFileWatcherTimer : IArkSaveFileWatcher
    {
        private Timer _timer;
        private readonly TimeSpan _delay = TimeSpan.FromSeconds(10);
        private DateTime _lastWrite;
        private ArkServerContext _serverContext;

        public event ArkSaveFileChangedEventHandler Changed;

        public ArkSaveFileWatcherTimer(ArkServerContext serverContext)
        {
            _serverContext = serverContext;

            _timer = new Timer(_timer_Callback, null, TimeSpan.Zero, _delay);
        }

        private void _timer_Callback(object state)
        {
            try
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                if (!File.Exists(_serverContext.Config.SaveFilePath)) return;

                var lastWrite = _lastWrite;
                _lastWrite = File.GetLastWriteTimeUtc(_serverContext.Config.SaveFilePath);
                if (lastWrite == DateTime.MinValue || lastWrite == _lastWrite) return;

                OnChanged();
            }
            catch (Exception ex)
            {
                Logging.LogException("Error in save file watcher (timer)", ex, this.GetType(), LogLevel.WARN, ExceptionLevel.Ignored);
            }
            finally
            {
                _timer.Change(_delay, _delay);
            }
        }

        private void OnChanged()
        {
            Changed?.Invoke(_serverContext, new ArkSaveFileChangedEventArgs { PathToLoad = _serverContext.Config.SaveFilePath });
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;

            if (disposing)
            {
                _timer?.Dispose();
                _timer = null;
            }

            disposedValue = true;
        }
        public void Dispose() { Dispose(true); }
        private bool disposedValue = false;
        #endregion
    }
}
