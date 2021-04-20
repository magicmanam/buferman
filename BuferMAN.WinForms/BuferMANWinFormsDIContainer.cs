using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Infrastructure.Window;
using BuferMAN.Windows;
using BuferMAN.WinForms.Window;
using SimpleInjector;

namespace BuferMAN.WinForms
{
    public class BufermanWinFormsDIContainer : BufermanWindowsDIContainer
    {
        public BufermanWinFormsDIContainer()
        {
            this.Register<IRenderingHandler, RenderingHandler>(Lifestyle.Singleton);// TODO (m) into BuferMAN.Application assembly
            this.Register<IBuferHandlersBinder, BuferHandlersBinder>(Lifestyle.Singleton);
            this.Register<IBufermanHost, BufermanWindow>(Lifestyle.Singleton);
            this.Register<IBufermanOptionsWindowFactory, BufermanOptionsWindowFactory>(Lifestyle.Singleton);
        }
    }
}
