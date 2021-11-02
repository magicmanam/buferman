using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Windows;
using SimpleInjector;

namespace BuferMAN.WinForms
{
    public class BufermanWinFormsDIContainer : BufermanWindowsDIContainer
    {
        public BufermanWinFormsDIContainer()
        {
            this.Register<IBuferHandlersBinder, BuferHandlersBinder>(Lifestyle.Singleton);
            this.Register<IBufermanHost, BufermanWindow>(Lifestyle.Singleton);
            this.Register<IBufermanOptionsWindowFactory, BufermanOptionsWindowFactory>(Lifestyle.Singleton);
        }
    }
}
