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
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class Configuration : UserControl
    {
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(Config), typeof(Configuration), new FrameworkPropertyMetadata());

        public Config Model
        {
            get { return GetValue(ModelProperty) as Config; }
            set { SetValue(ModelProperty, value); }
        }

        public Configuration()
        {
            InitializeComponent();
        }
    }
}
