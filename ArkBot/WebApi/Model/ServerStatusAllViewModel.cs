using System.Collections.Generic;

namespace ArkBot.WebApi.Model
{
    public class ServerStatusAllViewModel
    {
        public ServerStatusAllViewModel()
        {
            Servers = new List<ServerStatusViewModel>();
            Clusters = new List<ClusterStatusViewModel>();
        }

        public IList<ServerStatusViewModel> Servers { get; set; }
        public IList<ClusterStatusViewModel> Clusters { get; set; }
        public UserViewModel User { get; set; }
        public AccessControlViewModel AccessControl { get; set; }
    }
}
