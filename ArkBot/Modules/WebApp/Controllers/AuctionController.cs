using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.AuctionHouse;
using ArkBot.Modules.WebApp.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ArkBot.Modules.WebApp.Controllers
{
    [AccessControl("pages", "auction")]
    public class AuctionController : BaseApiController
    {
        private AuctionHouseManager _auctionHouseManager;

        public AuctionController(AuctionHouseManager auctionHouseManager, IConfig config) : base(config)
        {
            _auctionHouseManager = auctionHouseManager;
        }

        public List<Market> Get()
        {
            return _auctionHouseManager.Markets;
        }
    }
}
