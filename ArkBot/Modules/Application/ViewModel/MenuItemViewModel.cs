﻿using PropertyChanged;
using System.Windows.Input;

namespace ArkBot.Modules.Application.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class MenuItemViewModel
    {
        public string Header { get; set; }
        public ICommand Command { get; set; }
        public string CommandParameter { get; set; }
    }
}
