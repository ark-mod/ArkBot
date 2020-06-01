using System.Collections.Generic;

namespace ArkBot.Modules.WebApp.Model
{
    public class AccessControlViewModel : Dictionary<string, Dictionary<string, List<string>>>
    {
        public AccessControlViewModel()
        {
        }
    }
}
