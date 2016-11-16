using System.Windows;

namespace WebcamTesterWPF
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            new MainWindow(new MainViewModel()).Show();
        }
    }
}