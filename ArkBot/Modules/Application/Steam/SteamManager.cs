using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Utils;
using CoreRCON;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Modules.Application.Steam
{
    public class SteamManager : IDisposable
    {
        private ServerConfigSection _config;
        private RCON _rcon;

        private static SemaphoreSlim _rconCommandMutex = new SemaphoreSlim(1);

        public SteamManager(ServerConfigSection config)
        {
            _config = config;
        }

        public async Task Initialize()
        {
            _rcon = new RCON(IPAddress.Parse(_config.Ip), (ushort)_config.RconPort, _config.RconPassword);
            _rcon.OnDisconnected += _rcon_OnDisconnected;

            await Connect();
        }

        private async Task Connect()
        {
            try
            {
                await _rcon.ConnectAsync().ConfigureAwait(false);
            }
            // {"No connection could be made because the target machine actively refused it. 127.0.0.1:27020"}
            catch (System.Net.Sockets.SocketException ex)
            when (ex.Message?.Contains("No connection could be made because the target machine actively refused it") == true)
            {
                // can't connect
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Logging.LogException($"Exception attempting to connect to rcon server ({_config.Ip}:{_config.RconPort})", ex, typeof(SteamManager), LogLevel.DEBUG, ExceptionLevel.Ignored);
            }
        }

        private async void _rcon_OnDisconnected()
        {
            Debug.WriteLine("Connecting rcon...");
            await Connect();
        }

        /// <summary>
        /// Sends an admin command via rcon
        /// </summary>
        /// <returns>Result from server on success, null if failed.</returns>
        public async Task<string> SendRconCommand(string command)
        {
            // wait for other commands to finish before next
            // this is to avoid problems with the order of messages returned from the rcon api

            // todo: maybe pool some connections to allow a few concurrent calls
            await _rconCommandMutex.WaitAsync().ConfigureAwait(false);

            try
            {
                return await SendRconCommandInternal(command);
            }
            finally
            {
                _rconCommandMutex.Release();
            }
        }

        private async Task<string> SendRconCommandInternal(string command, int currentRetryCount = 0)
        {
            try
            {
                var result = await _rcon.SendCommandAsync(command).ConfigureAwait(false);
                return result;
            }
            catch (InvalidOperationException ex)
            when (ex.Message?.Contains("Connection is closed") == true)
            {
                if (currentRetryCount >= 1)
                {
                    Logging.LogException($"Exception attempting to send rcon command after reconnect attempt ({currentRetryCount})", ex, typeof(SteamManager), LogLevel.DEBUG, ExceptionLevel.Ignored);
                    return null;
                }

                await Connect();
                await Task.Delay(500); // sending rcon commands directly after connecting gets connection stuck at receiving reply
                await SendRconCommandInternal(command, currentRetryCount + 1);
            }
            catch (Exception ex)
            {
                Logging.LogException("Exception attempting to send rcon command", ex, typeof(SteamManager), LogLevel.DEBUG, ExceptionLevel.Ignored);
            }

            return null;
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
                    _rcon?.Dispose();
                    _rcon = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SteamManager()
        // {
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