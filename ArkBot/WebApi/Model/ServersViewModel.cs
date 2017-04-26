using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class ServersViewModel
    {
        public ServersViewModel()
        {
            Servers = new List<ServerViewModel>();
            Clusters = new List<ClusterViewModel>();
        }

        public IList<ServerViewModel> Servers { get; set; }
        public IList<ClusterViewModel> Clusters { get; set; }
    }
}
