using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.ViewModel
{
    public abstract class TabViewModel : ToolViewModel
    {
        public TabViewModel(string contentId, string name) : base(contentId, name)
        {
        }
    }
}
