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
            Servers = new List<PlayerServerViewModel>();
        }

        public IList<PlayerServerViewModel> Servers { get; set; }
    }
}
