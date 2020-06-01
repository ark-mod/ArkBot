using System;

namespace ArkBot.Modules.WebApp.Model
{
    public class ServerStatusViewModel
    {
        //public ServerStatusViewModel()
        //{
        //    OnlinePlayers = new List<OnlinePlayerViewModel>();
        //}

        public string Key { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Version { get; set; }
        public string MapName { get; set; }
        public string LastUpdate { get; set; }
        public string NextUpdate { get; set; }
        //public int OnlinePlayerCount { get; set; }
        public int OnlinePlayerMax { get; set; }

        public string InGameTime { get; set; }
        public int TamedCreatureCount { get; set; }
        public int CloudCreatureCount { get; set; }
        public int WildCreatureCount { get; set; }
        public int StructureCount { get; set; }
        public int PlayerCount { get; set; }
        public int TribeCount { get; set; }
        public DateTime? ServerStarted { get; set; }

        // access control (home / online)
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public IList<OnlinePlayerViewModel> OnlinePlayers { get; set; }
    }
}
