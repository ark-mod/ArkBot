using ArkBot.Modules.Application;
using ArkBot.Modules.Application.Configuration.Model;
using ArkBot.Modules.AuctionHouse;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArkBot.Modules.WebApp.Hubs
{
    public interface IServerUpdateClient
    {
        Task ServerUpdate(string serverKey);
        Task ClusterUpdate(string clusterKey);
        Task MarketUpdate(string name);
        Task OnlinePlayers(Dictionary<string, OnlinePlayerViewModel[]> onlinePlayers);
        Task PlayerLocations(Dictionary<string, PlayerLocationViewModel[]> playerLocations);
        Task ChatMessages(ChatMessageViewModel[] msg);
    }

    public class ServerUpdateHub : Hub<IServerUpdateClient>
    {
        private ArkContextManager _contextManager;
        private NotificationManager _notificationManager;
        private IConfig _config;

        public ServerUpdateHub(ArkContextManager contextManager, NotificationManager notificationManager, IConfig config)
        {
            _contextManager = contextManager;
            _notificationManager = notificationManager;
            _config = config;
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            await _notificationManager.SendOnlinePlayersInternal(Clients.Caller);
            await _notificationManager.SendChatMessagesInternal(Clients.Caller);
        }
    }

    public class OnlinePlayerViewModel
    {
        public ulong SteamId { get; set; }
        public string SteamName { get; set; }
        public string CharacterName { get; set; }
        public string TribeName { get; set; }
        public string DiscordName { get; set; }
        public DateTime LoginTime { get; set; }
    }

    public class PlayerLocationViewModel
    {
        public ulong SteamId { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float? TopoMapX { get; set; }
        public float? TopoMapY { get; set; }
    }

    public class ChatMessageViewModel
    {
        public DateTime At { get; set; }
        public string ServerKey { get; set; }
        public ulong SteamId { get; set; }
        public string PlayerName { get; set; }
        public string TribeName { get; set; }
        public string Message { get; set; }
        public ChatIcon Icon { get; set; }
    }
}
