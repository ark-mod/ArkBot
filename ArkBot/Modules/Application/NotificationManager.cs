using ArkBot.Modules.AuctionHouse;
using ArkBot.Modules.Database;
using ArkBot.Modules.WebApp.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArkBot.Modules.Application
{
    public class NotificationManager
    {
        private ArkContextManager _contextManager;
        private EfDatabaseContextFactory _databaseContextFactory;
        private IHubContext<ServerUpdateHub, IServerUpdateClient> _hubContext;

        internal IHubContext<ServerUpdateHub, IServerUpdateClient> WebAppHub { get => _hubContext; }

        public NotificationManager(ArkContextManager contextManager, EfDatabaseContextFactory databaseContextFactory)
        {
            _contextManager = contextManager;
            _databaseContextFactory = databaseContextFactory;

            _contextManager.GameDataUpdated += _contextManager_GameDataUpdated;
        }

        internal void Setup(IHubContext<ServerUpdateHub, IServerUpdateClient> hubContext)
        {
            _hubContext = hubContext;
        }

        /// <summary>
        /// When gamedata changes broadcast SignalR clients with serverUpdateNotification-/clusterUpdateNotification-message
        /// </summary>
        private async void _contextManager_GameDataUpdated(IArkUpdateableContext sender)
        {
            if (_hubContext == null) return;

            if (sender is ArkServerContext) await _hubContext.Clients.All.ServerUpdate((sender as ArkServerContext).Config.Key);
            if (sender is ArkClusterContext) await _hubContext.Clients.All.ClusterUpdate((sender as ArkClusterContext).Config.Key);
        }

        internal async Task SendOnlinePlayersInternal(IServerUpdateClient client = null)
        {
            if (!_contextManager.IsFullyInitialized) return;
            if ((client = client ?? _hubContext?.Clients.All) == null) return;

            using var db = _databaseContextFactory.Create();

            var serverKeys = _contextManager.Servers.Select(x => x.Config.Key).ToArray();
            var result = (await db.Players.Where(x => x.IsOnline && serverKeys.Contains(x.LastServerKey)).ToArrayAsync()).GroupBy(x => x.LastServerKey).ToDictionary(x => x.Key, x =>
            {
                var context = _contextManager.GetServer(x.Key);
                return x.Select(y =>
                {
                    var p = context.Players.FirstOrDefault(z => z.SteamId.Equals(y.Id.ToString()));
                    var vm = new OnlinePlayerViewModel
                    {
                        SteamId = y.Id,
                        SteamName = p?.Name ?? y.Id.ToString(),
                        CharacterName = p?.CharacterName,
                        TribeName = p?.Tribe?.Name,
                        DiscordName = null,
                        LoginTime = y.LastLogin
                    };
                    return vm;
                }).ToArray();
            });

            await client.OnlinePlayers(result);
        }

        internal async Task SendPlayerLocationsInternal(IServerUpdateClient client = null)
        {
            if (!_contextManager.IsFullyInitialized) return;
            if ((client = client ?? _hubContext?.Clients.All) == null) return;

            using var db = _databaseContextFactory.Create();

            var serverKeys = _contextManager.Servers.Select(x => x.Config.Key).ToArray();
            var db_result = await db.Players.Where(x => x.IsOnline).Select(x => x.LoggedLocations.OrderByDescending(y => y.At).FirstOrDefault()).ToArrayAsync();

            var result = db_result.Where(x => serverKeys.Contains(x.ServerKey)).GroupBy(x => x.ServerKey).ToDictionary(x => x.Key, x =>
            {
                var context = _contextManager.GetServer(x.Key);
                return x.Select(y =>
                {
                    var p = context.Players.FirstOrDefault(z => z.SteamId.Equals(y.Id.ToString()));
                    var loc = new ArkSavegameToolkitNet.Domain.ArkLocation(
                                        new ArkSavegameToolkitNet.DataTypes.Extras.LocationData { x = y.X, y = y.Y },
                                        new ArkSavegameToolkitNet.DataTypes.ArkSaveData { mapName = context.SaveState.MapName }
                                        );

                    var vm = new PlayerLocationViewModel
                    {
                        SteamId = y.SteamId,
                        Latitude = y.Latitude,
                        Longitude = y.Longitude,
                        TopoMapX = loc.TopoMapX,
                        TopoMapY = loc.TopoMapY
                    };
                    return vm;
                }).ToArray();
            });

            await client.PlayerLocations(result);
        }

        internal async Task SendChatMessagesInternal(IServerUpdateClient client = null)
        {
            if (!_contextManager.IsFullyInitialized) return;
            if ((client = client ?? _hubContext?.Clients.All) == null) return;

            using var db = _databaseContextFactory.Create();

            var serverKeys = _contextManager.Servers.Select(x => x.Config.Key).ToArray();
            var result = await db.ChatMessages.Where(x => serverKeys.Contains(x.ServerKey)).OrderByDescending(x => x.At).Take(25).Select(x => new ChatMessageViewModel
            {
                At = x.At,
                ServerKey = x.ServerKey,
                SteamId = x.SteamId,
                PlayerName = x.PlayerName,
                TribeName = x.TribeName,
                Message = x.Message,
                Icon = x.Icon
            }).OrderBy(x => x.At).ToArrayAsync();

            await client.ChatMessages(result);
        }

        internal async Task SendChatMessageInternal(ChatMessageViewModel msg, IServerUpdateClient client = null)
        {
            if ((client = client ?? _hubContext?.Clients.All) == null) return;

            await client.ChatMessages(new ChatMessageViewModel[] { msg });
        }
    }
}
