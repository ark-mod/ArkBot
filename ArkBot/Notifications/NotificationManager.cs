using ArkBot.Ark;
using ArkBot.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace ArkBot.Notifications
{
    public class NotificationManager
    {
        private ArkContextManager _contextManager;
        private IHubContext<ServerUpdateHub> _hubContext;

        public NotificationManager(ArkContextManager contextManager, IHubContext<ServerUpdateHub> hubContext)
        {
            _contextManager = contextManager;
            _hubContext = hubContext;

            _contextManager.GameDataUpdated += _contextManager_GameDataUpdated;
        }

        /// <summary>
        /// When gamedata changes broadcast SignalR clients with serverUpdateNotification-/clusterUpdateNotification-message
        /// </summary>
        private async void _contextManager_GameDataUpdated(IArkUpdateableContext sender)
        {
            
            if (sender is ArkServerContext) await _hubContext.Clients.All.SendAsync("serverUpdateNotification", (sender as ArkServerContext).Config.Key);
            if (sender is ArkClusterContext) await _hubContext.Clients.All.SendAsync("clusterUpdateNotification", (sender as ArkClusterContext).Config.Key);
        }
    }
}
