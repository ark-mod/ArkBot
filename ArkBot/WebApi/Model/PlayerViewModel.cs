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
        }

        public IDictionary<string, PlayerServerViewModel> Servers { get; set; }
    }
}
