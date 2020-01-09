using System.Collections.Generic;

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
