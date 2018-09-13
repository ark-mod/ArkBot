using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkBot.ViewModel
{
    public abstract class ToolViewModel : PaneViewModel
    {
        public ToolViewModel(string contentId, string name) : base(contentId, name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        public bool IsVisible { get; set; }
    }
}
