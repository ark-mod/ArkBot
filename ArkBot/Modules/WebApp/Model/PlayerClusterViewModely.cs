using System.Collections.Generic;

namespace ArkBot.Modules.WebApp.Model
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
