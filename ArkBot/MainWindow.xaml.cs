using ArkBot.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArkBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            Initialized += MainWindow_Initialized;

            InitializeComponent();

            Title = $"ARK Bot {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}";

            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            Unloaded += new RoutedEventHandler(MainWindow_Unloaded);
        }

        private async void MainWindow_Initialized(object sender, EventArgs e)
        {
            DataContext = await Workspace.AsyncInstance;

            //Check if the UI should be hidden on startup
            //Only checks here to allow the user to see if an error occurred
            if (Workspace.Instance._startedWithoutErrors && Workspace.Instance._config != null && Workspace.Instance._config.HideUiOnStartup)
            {
                Application.Current.MainWindow?.Hide();
                Workspace.Instance._isUIHidden = true;
            }

        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                if (File.Exists(Workspace.Constants.LayoutFilePath))
                {
                    var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer(dockManager);
                    //serializer.LayoutSerializationCallback += (s, args) =>
                    //{
                    //};

                    serializer.Deserialize(Workspace.Constants.LayoutFilePath);
                }
            }
            catch (Exception ex) { /*do nothing*/ }
        }

        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            var serializer = new Xceed.Wpf.AvalonDock.Layout.Serialization.XmlLayoutSerializer(dockManager);
            serializer.Serialize(Workspace.Constants.LayoutFilePath);

            Workspace.Instance.Dispose();
        }
    }
}
