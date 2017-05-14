using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
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
