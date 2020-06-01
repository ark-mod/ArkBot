namespace ArkBot.Modules.WebApp.Model
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
