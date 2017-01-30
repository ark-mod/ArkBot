using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot
{
    public class ArkSaveFileWatcher : IDisposable
    {
        private FileSystemWatcher _watcher;
        private string _saveFilePath;

        public string SaveFilePath
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

        public delegate void ArkSaveFileChangedEventHandler(object sender, ArkSaveFileChangedEventArgs e);

        public DateTime? LastChanged { get; private set; }
        public event ArkSaveFileChangedEventHandler Changed;

        public ArkSaveFileWatcher()
        {
            _watcher = new FileSystemWatcher();
            _watcher.Changed += _watcher_Changed;
            _watcher.Created += _watcher_Changed;

            updateWatcher();
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
            if(Changed != null) Changed(this, new ArkSaveFileChangedEventArgs { SaveFileName = SaveFilePath });
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _watcher?.Dispose();
                    _watcher = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ArkSaveFileWatcher() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
