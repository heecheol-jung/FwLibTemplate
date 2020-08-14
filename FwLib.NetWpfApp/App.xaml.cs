using log4net;
using log4net.Config;
using System.IO;
using System.Reflection;
using System.Windows;

namespace FwLib.NetWpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("FwLib.NetWpfApp_log_config.xml"));
        }
    }
}
