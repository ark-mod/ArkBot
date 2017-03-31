using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArkBot.Controls
{
    /// <summary>
    /// Interaction logic for Console.xaml
    /// </summary>
    public partial class Console : UserControl
    {
        public static readonly DependencyProperty ConsoleOutputProperty = DependencyProperty.Register("ConsoleOutput", typeof(IList<string>), typeof(Console), new FrameworkPropertyMetadata(new string[] { }));

        public IList<string> ConsoleOutput
        {
            get { return GetValue(ConsoleOutputProperty) as IList<string>; }
            set { SetValue(ConsoleOutputProperty, value); }
        }

        public Console()
        {
            InitializeComponent();
        }
    }
}
