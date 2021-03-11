using ArkBot.Modules.Application;
using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Utils;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkBot.Modules.AuctionHouse
{
    public class AuctionHouseManager
    {
        public List<Market> Markets { get; set; } = new List<Market>();

        private IConfig _config;
        private Timer _timer;

        private const int INTERVAL = 60000;

        public AuctionHouseManager(IConfig config)
        {
            _config = config;
        }

        private async void UpdateTick()
        {
            // not cached?
            await Update();
            _timer.Change(INTERVAL, Timeout.Infinite);
        }

        public async void Start()
        {
            try
            {
                await Update();
                _timer = new Timer(_ => UpdateTick(), null, INTERVAL, Timeout.Infinite);
            }
            catch { }
        }

        private async Task<bool> Update()
        {
            var cached = true;
            foreach (var ah in _config.AuctionHouse.Markets)
            {
                try
                {
                    var client = new HttpClient();
                    var requestUri = "https://linode.ghazlawl.com/ark/mods/auctionhouse/api/json/v1/auctions/";
                    requestUri = QueryHelpers.AddQueryString(requestUri, "MarketID", ah.MarketId);
                    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                    request.Headers.Add("Accept", "application/json");
                    var response = await client.GetAsync(requestUri);
                    var json = await response.Content.ReadAsStringAsync();

                    var existing = Markets.FirstOrDefault(x => x.Name == ah.Name);
                    Markets.Remove(existing);

                    var market = Newtonsoft.Json.JsonConvert.DeserializeObject<Market>(json);
                    market.Name = ah.Name;
                    Markets.Add(market);
                }
                catch (Exception ex)
                {
                    Logging.LogException($"Failed to update auction house ({ah.Name})", ex, GetType(), LogLevel.ERROR, ExceptionLevel.Ignored);
                }
            }

            return cached;
        }
    }
}
