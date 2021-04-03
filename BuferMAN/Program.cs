using BuferMAN.Infrastructure.Environment;
using BuferMAN.WinForms;
using log4net.Config;
using log4net.Repository.Hierarchy;
using Logging;
using System;

namespace BuferMAN
{
	static class Program
    {
        [STAThread]
        static void Main()
        {
            Program._ConfigureLogging();

            var container = new BufermanWinFormsDIContainer();

            var starter = container.GetInstance<IStarter>();
            starter.EnsureOneInstanceStart();
        }

        private static void _ConfigureLogging()
        {
            XmlConfigurator.Configure();//Note
            Logging.Logger.Current = new Log4netLogger();
            //Logger.Logger.Current = new ConsoleLogger();
        }
    }
}
