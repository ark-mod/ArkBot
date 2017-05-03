//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace ArkBot
//{
//    public class DebugFakeSaveFileWatcher : IArkSaveFileWatcher
//    {
//        public event ArkSaveFileChangedEventHandler Changed;
//        private Timer _timer;
//        private TimeSpan _delay;
//        private int _index = 0;
//        private IConfig _config;

//        public DebugFakeSaveFileWatcher(IConfig config, TimeSpan? delay = null)
//        {
//            _config = config;
//            _delay = delay ?? TimeSpan.FromMinutes(1);
//            _timer = new Timer(new TimerCallback(_timer_callback), null, _delay, _delay);
//        }

//        public void OnChanged()
//        {
//            var continueRunning = true;
//            try
//            {
//                _timer.Change(Timeout.Infinite, Timeout.Infinite);
//                _index += 1;

//                if (_config.DebugNoExtract)
//                {
//                    var path = _config.DebugJsonOutputDirPath?.Replace("{index}", _index.ToString());
//                    if (path == null || !Directory.Exists(path))
//                    {
//                        continueRunning = false;
//                        return;
//                    }

//                    Changed?.Invoke(null, new ArkSaveFileChangedEventArgs { PathToLoad = path });
//                }
//                else
//                {
//                    var path = _config.DebugSaveFilePath?.Replace("{index}", _index.ToString());
//                    var clusterpath = _config.DebugClusterSavePath?.Replace("{index}", _index.ToString());
//                    if (path == null || !File.Exists(path) || clusterpath == null || !Directory.Exists(clusterpath))
//                    {
//                        continueRunning = false;
//                        return;
//                    }

//                    Changed?.Invoke(null, new ArkSaveFileChangedEventArgs { SaveFilePath = path, ClusterPath = clusterpath });
//                }
//            }
//            catch { /*ignore all exceptions*/ }
//            finally
//            {
//                if (continueRunning) _timer.Change(_delay, _delay);
//            }
//        }

//        private void _timer_callback(object state)
//        {
//            OnChanged();
//        }

//        #region IDisposable Support
//        private bool disposedValue = false;

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//                }
//                disposedValue = true;
//            }
//        }

//        public void Dispose()
//        {
//            Dispose(true);
//        }
//        #endregion
//    }
//}
