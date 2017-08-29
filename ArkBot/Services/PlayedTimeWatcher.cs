//using ArkBot.Helpers;
//using QueryMaster.GameServer;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace ArkBot.Services
//{
//    public class PlayedTimeWatcher : IPlayedTimeWatcher
//    {
//        private bool _isStarted;
//        private Timer _timer;
//        private readonly TimeSpan _delay = TimeSpan.FromMinutes(5);
//        private DateTime? _lastCheck = null;
//        private string[] _lastOnline;

//        private IConfig _config;

//        public delegate void PlayedTimeUpdateEventHandler(object sender, PlayedTimeEventArgs e);
//        public event PlayedTimeUpdateEventHandler PlayedTimeUpdate;


//        public PlayedTimeWatcher(IConfig config)
//        {
//            _config = config;
//            _lastOnline = new string[] { };

//            _timer = new Timer(_timer_Callback, null, Timeout.Infinite, Timeout.Infinite);
//        }

//        public void Start()
//        {
//            if (_isStarted) return;

//            _isStarted = true;
//            _timer.Change(TimeSpan.Zero, _delay);
//        }

//        private void _timer_Callback(object state)
//        {
//            try
//            {
//                _timer.Change(Timeout.Infinite, Timeout.Infinite);

//                using (var server = ServerQuery.GetServerInstance(
//                    QueryMaster.EngineType.Source, 
//                    _config.ServerIp, 
//                    (ushort)_config.ServerPort, 
//                    throwExceptions: false, 
//                    retries: 2, 
//                    sendTimeout: 4000, 
//                    receiveTimeout: 4000))
//                {
//                    var playerInfo = server.GetPlayers();
//                    var players = playerInfo?.Select(x => x.Name).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
//                    if (players == null || players.Length <= 0) return;

//                    var online = players.Intersect(_lastOnline, StringComparer.Ordinal).ToArray();

//                    var lastCheck = _lastCheck;
//                    _lastCheck = DateTime.Now;
//                    _lastOnline = players;

//                    if (online.Length <= 0 || lastCheck == null) return;

//                    var played = _lastCheck.Value - lastCheck.Value;
//                    PlayedTimeUpdate?.Invoke(this, new PlayedTimeEventArgs { Players = online, Date = _lastCheck.Value, TimeToAdd = played });
//                }
//            }
//            finally
//            {
//                _timer.Change(_delay, _delay); 
//            }
//        }

//        public class PlayedTimeEventArgs : EventArgs
//        {
//            public string[] Players { get; set; }
//            public TimeSpan TimeToAdd { get; set; }
//            public DateTime Date { get; set; }
//        }

//        #region IDisposable Support
//        private bool disposedValue = false;

//        protected virtual void Dispose(bool disposing)
//        {
//            if (!disposedValue)
//            {
//                if (disposing)
//                {
//                    _timer?.Dispose();
//                    _timer = null;
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
