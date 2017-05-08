using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class ClusterStatusViewModel
    {
        public ClusterStatusViewModel()
        {
            ServerKeys = new string[] { };
        }

        public string Key { get; set; }
        public string[] ServerKeys { get; set; }
    }
}
