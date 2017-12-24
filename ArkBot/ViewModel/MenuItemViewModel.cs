using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ArkBot.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class MenuItemViewModel
    {
        public string Header { get; set; }
        public ICommand Command { get; set; }
        public string CommandParameter { get; set; }
    }
}
