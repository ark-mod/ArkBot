using ArkBot.Commands;
using ArkBot.Helpers;
using Newtonsoft.Json;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArkBot.ViewModel
{
    public class ConfigurationViewModel : TabViewModel
    {
        public Config Config { get; set; }

        public ConfigurationViewModel() : base("Configuration", "Configuration")
        {
        }
    }
}
