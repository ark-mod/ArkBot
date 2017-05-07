using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class BulkViewModel
    {
        public BulkViewModel()
        {
            Servers = new Dictionary<string, List<PlayerServerViewModel>>();
        }

        public IDictionary<string, List<PlayerServerViewModel>> Servers { get; set; }
    }
}
