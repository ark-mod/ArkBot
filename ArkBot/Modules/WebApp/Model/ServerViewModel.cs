using System.Collections.Generic;

namespace ArkBot.Modules.WebApp.Model
{
    public class ServerViewModel
    {
        public ServerViewModel()
        {
            Players = new List<PlayerReferenceViewModel>();
            Tribes = new List<TribeReferenceViewModel>();
        }
        public List<PlayerReferenceViewModel> Players { get; set; }
        public List<TribeReferenceViewModel> Tribes { get; set; }
        public string MapName { get; set; }
    }
}
