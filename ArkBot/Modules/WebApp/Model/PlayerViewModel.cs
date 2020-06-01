using System.Collections.Generic;

namespace ArkBot.Modules.WebApp.Model
{
    public class PlayerViewModel
    {
        public PlayerViewModel()
        {
            Servers = new Dictionary<string, PlayerServerViewModel>();
            Clusters = new Dictionary<string, PlayerClusterViewModel>();
            MapNames = new Dictionary<string, string>();
        }

        public IDictionary<string, PlayerServerViewModel> Servers { get; set; }
        public IDictionary<string, PlayerClusterViewModel> Clusters { get; set; }
        public IDictionary<string, string> MapNames { get; set; }
    }
}
