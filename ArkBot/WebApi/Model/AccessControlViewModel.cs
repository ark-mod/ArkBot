using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.WebApi.Model
{
    public class AccessControlViewModel : Dictionary<string, Dictionary<string, List<string>>>
    {
        public AccessControlViewModel()
        {
        }
    }
}
