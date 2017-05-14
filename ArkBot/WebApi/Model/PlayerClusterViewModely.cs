using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class PlayerClusterViewModel
    {
        public PlayerClusterViewModel()
        {
            Creatures = new List<CloudCreatureViewModel>();
        }

        public List<CloudCreatureViewModel> Creatures { get; set; }
    }
}
