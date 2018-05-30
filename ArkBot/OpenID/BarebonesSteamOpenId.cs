using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ArkBot.OpenID
{
    public class BarebonesSteamOpenId : IBarebonesSteamOpenId
    {
        private const string _authority = @"https://steamcommunity.com/openid";
        private AsyncLazy<string> _endpoint = new AsyncLazy<string>(async () => await Discovery());
        private HttpListener _listener;
        private CancellationTokenSource _cts;
        private Task _service;
        private ConcurrentDictionary<Guid, Task> _ongoingTasks;
        private ConcurrentDictionary<Guid, SteamOpenIdState> _states;

        private SteamOpenIdOptions _options;
        private Func<bool, ulong, ulong, Task<string>> _getHtmlContent;

        public delegate void SteamOpenIdCallbackEventHandler(object sender, SteamOpenIdCallbackEventArgs e);
        public event SteamOpenIdCallbackEventHandler SteamOpenIdCallback;

        private void OnSteamOpenIdCallback(bool successful, ulong steamId, ulong discordUserId)
        {
            SteamOpenIdCallback?.Invoke(this, new SteamOpenIdCallbackEventArgs { Successful = successful, SteamId = steamId, DiscordUserId = discordUserId });
        }

        public BarebonesSteamOpenId(SteamOpenIdOptions options, Func<bool, ulong, ulong, Task<string>> getHtmlContent)
        {
            _options = options;
            _getHtmlContent = getHtmlContent;
            _ongoingTasks = new ConcurrentDictionary<Guid, Task>();
            _states = new ConcurrentDictionary<Guid, SteamOpenIdState>();

            StartService();
        }

        private async Task StartService()
        {
            _listener = new HttpListener();
            foreach (var prefix in _options.ListenPrefixes) _listener.Prefixes.Add(prefix);
            _listener.Start();

            _cts = new CancellationTokenSource();

            _service = Task.Factory.StartNew(async () =>
            {
                _cts.Token.ThrowIfCancellationRequested();
                while (true)
                {
                    if (_cts.Token.IsCancellationRequested) _cts.Token.ThrowIfCancellationRequested();
                    var context = await _listener.GetContextAsync();
                    if (context != null) _ongoingTasks.TryAdd(context.Request.RequestTraceIdentifier, HandleRequestAsync(context));

                    foreach (var task in _ongoingTasks.Where(x => x.Value.IsCanceled || x.Value.IsCompleted || x.Value.IsFaulted).ToArray())
                    {
                        Task t;
                        _ongoingTasks.TryRemove(task.Key, out t);
                    }

                    foreach (var state in _states.Where(x => (DateTime.Now - x.Value.When) >= TimeSpan.FromMinutes(5)).ToArray())
                    {
                        SteamOpenIdState s;
                        _states.TryRemove(state.Key, out s);
                    }
                }
            }, _cts.Token).ContinueWith(async task =>
            {
                await Task.WhenAll(_ongoingTasks.Values.ToArray());
            });
        }

        public async Task<SteamOpenIdState> LinkWithSteamTaskAsync(ulong discordUserId)
        {
            var a = Guid.NewGuid();
            var state = new SteamOpenIdState
            {
                ReturnTo = new Uri(new Uri(_options.RedirectUri), $"?a={a}"),
                Identity = @"http://specs.openid.net/auth/2.0/identifier_select",
                ClaimedId = @"http://specs.openid.net/auth/2.0/identifier_select",
                Authority = _authority,
                DiscordUserId = discordUserId,
            };
            if (!_states.TryAdd(a, state)) return null;

            //&openid.realm={state.Realm}
            var uri = new Uri(new Uri(await _endpoint), $@"?openid.ns=http://specs.openid.net/auth/2.0&openid.mode=checkid_setup&openid.return_to={state.ReturnTo}&openid.claimed_id={state.ClaimedId}&openid.identity={state.Identity}");
            state.StartUrl = uri.ToString();

            return state;
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var successful = false;
            ulong steamId = 0;
            SteamOpenIdState state = null;
            try
            {
                var request = context.Request;
                var qs = request.QueryString;
                Guid a;
                if (!Guid.TryParse(qs["a"] ?? "", out a))
                {
                    //request failed
                    return;
                }

                
                if (!_states.TryRemove(a, out state))
                {
                    //failed to get state
                    return;
                }

                var query = UriExtensions.ParseQueryString(state.ReturnTo);

                if (request.Url.Scheme != state.ReturnTo.Scheme
                    || request.Url.Authority != state.ReturnTo.Authority
                    || request.Url.AbsolutePath != state.ReturnTo.AbsolutePath
                    || query.AllKeys.Any(x => qs[x] == null || qs[x].Equals(query[x], StringComparison.Ordinal) == false)
                    || qs["openid.mode"]?.Equals("id_res") != true
                    || qs["openid.claimed_id"]?.StartsWith(state.Authority) != true)
                {
                    //assertions failed
                    return;
                }

                var r = new Regex(@"/openid/id/(?<steamid>\d+)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                var m = r.Match(qs["openid.claimed_id"]);
                if (!m.Success)
                {
                    //failed to get steamid
                    return;
                }

                if (!ulong.TryParse(m.Groups["steamid"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out steamId)) return;
                successful = true;

                //openid/
                //?openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0
                //&openid.mode=id_res
                //&openid.op_endpoint=https%3A%2F%2Fsteamcommunity.com%2Fopenid%2Flogin
                //&openid.claimed_id=http%3A%2F%2Fsteamcommunity.com%2Fopenid%2Fid%2F76561198005371608
                //&openid.identity=http%3A%2F%2Fsteamcommunity.com%2Fopenid%2Fid%2F76561198005371608
                //&openid.return_to=http%3A%2F%2F62.63.229.45%3A7331%2Fopenid%2F
                //&openid.response_nonce=2017-02-05T18%3A20%3A46ZwFvmnCumgCkT9IWOoIDpaJLc6%2Bs%3D
                //&openid.assoc_handle=1234567890
                //&openid.signed=signed%2Cop_endpoint%2Cclaimed_id%2Cidentity%2Creturn_to%2Cresponse_nonce%2Cassoc_handle
                //&openid.sig=Cpo83xgHcMqzVOyQqe693s%2FRQO4%3D
            }
            finally
            {
                Task task;
                _ongoingTasks.TryRemove(context.Request.RequestTraceIdentifier, out task);
                if (state != null)
                {
                    OnSteamOpenIdCallback(successful, steamId, state.DiscordUserId);

                    if (_getHtmlContent != null)
                    {
                        var content = await _getHtmlContent(successful, steamId, state.DiscordUserId);
                        var buffer = Encoding.UTF8.GetBytes(content);
                        context.Response.ContentLength64 = buffer.Length;
                        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Close();
                    }
                }
            }
        }

        private static async Task<string> Discovery()
        {
            using (var wc = new WebClient())
            {
                var data = await wc.DownloadStringTaskAsync(_authority);
                if (data == null) return null;
                var doc = XDocument.Parse(data);
                var serviceElement = doc?.Document.Descendants()
                    .FirstOrDefault(x => x.Name.LocalName.Equals("Service")
                        && x.Descendants().Any(y => y.Name.LocalName.Equals("Type")
                            && y.Value.Equals("http://specs.openid.net/auth/2.0/server", StringComparison.OrdinalIgnoreCase)));
                var opidentifier = serviceElement?.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("URI"))?.Value;
                return opidentifier;
            }
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
                    _cts.Cancel();
                    _listener.Close();
                    _listener = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SimpleSteamOpenID() {
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
