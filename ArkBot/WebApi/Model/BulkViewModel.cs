using System.Collections.Generic;

namespace ArkBot.WebApi.Model
{
    public class BulkViewModel
    {
        public BulkViewModel()
        {
            Servers = new Dictionary<string, List<PlayerServerViewModel>>();
            MapNames = new Dictionary<string, string>();
        }

        public IDictionary<string, List<PlayerServerViewModel>> Servers { get; set; }
        public IDictionary<string, string> MapNames { get; set; }
    }
}
