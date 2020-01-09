using System.Collections.Generic;

namespace ArkBot.WebApi.Model
{
    public class AccessControlViewModel : Dictionary<string, Dictionary<string, List<string>>>
    {
        public AccessControlViewModel()
        {
        }
    }
}
