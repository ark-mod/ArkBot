using ArkBot.Ark;
using ArkBot.WebApi.Hubs;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Notifications
{
    public class NotificationManager
    {
        private ArkContextManager _contextManager;
        private IDependencyResolver _dependencyResolver;
        private IConnectionManager _connectionManager;
        private IHubContext _hubContext;

        public NotificationManager(ArkContextManager contextManager, IDependencyResolver dependencyResolver)
        {
            _contextManager = contextManager;
            _dependencyResolver = dependencyResolver;
            _connectionManager = _dependencyResolver.Resolve<IDependencyResolver>().Resolve<IConnectionManager>();
            _hubContext = _connectionManager.GetHubContext<ServerUpdateHub>();

            _contextManager.UpdateCompleted += _contextManager_UpdateCompleted;
        }

        /// <summary>
        /// On server updates broadcast SignalR clients with serverUpdateNotification-message
        /// </summary>
        private void _contextManager_UpdateCompleted(ArkServerContext sender, bool successful, bool cancelled)
        {
            if (!successful) return;

            _hubContext.Clients.All.serverUpdateNotification(sender.Config.Key);
        }
    }
}
