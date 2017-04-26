using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.Ark
{
    public class ArkClusterContext
    {
        public ClusterConfigSection Config { get; set; }

        public ArkClusterContext(ClusterConfigSection config)
        {
            Config = config;
        }
    }
}
