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
    public class ArkSaveFileWatcher : IArkSaveFileWatcher
    {
        private FileSystemWatcher _watcher;
        private string _saveFilePath;
        private ArkServerContext _serverContext;

        private string SaveFilePath
        {
            get
            {
                return _saveFilePath;
            }

            set
            {
                if ((_saveFilePath == null && value == null) 
                    || (_saveFilePath != null && _saveFilePath.Equals(_saveFilePath, StringComparison.OrdinalIgnoreCase))) return;
                _saveFilePath = value;
                updateWatcher();
            }
        }

        private DateTime? LastChanged { get; set; }
        public event ArkSaveFileChangedEventHandler Changed;

        public ArkSaveFileWatcher(ArkServerContext serverContext)
        {
            _serverContext = serverContext;

            _watcher = new FileSystemWatcher();
            _watcher.Changed += _watcher_Changed;
            _watcher.Created += _watcher_Changed;

            SaveFilePath = serverContext.Config.SaveFilePath;
        }

        private void updateWatcher()
        {

            if (string.IsNullOrWhiteSpace(SaveFilePath) || !File.Exists(SaveFilePath))
            {
                _watcher.EnableRaisingEvents = false;
            }
            else
            {
                _watcher.Path = Directory.GetParent(SaveFilePath).FullName;
                _watcher.Filter = Path.GetFileName(SaveFilePath);
                _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime;
                _watcher.EnableRaisingEvents = true;
            }
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!LastChanged.HasValue || (DateTime.Now - LastChanged.Value).TotalSeconds > 2)
            {
                OnChanged();
            }
        }

        private void OnChanged()
        {
            LastChanged = DateTime.Now;
            Changed?.Invoke(_serverContext, new ArkSaveFileChangedEventArgs { PathToLoad = SaveFilePath });
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;

            if (disposing)
            {
                _watcher?.Dispose();
                _watcher = null;
            }

            disposedValue = true;
        }
        public void Dispose() { Dispose(true); }
        private bool disposedValue = false;
        #endregion
    }
}
