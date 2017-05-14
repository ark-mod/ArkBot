using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class CloudCreatureViewModel
    {
        public CloudCreatureViewModel()
        {
            Aliases = new string[] { };
        }

        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Species { get; set; }
        public string[] Aliases { get; set; }
        public int? Level { get; set; }
    }
}
