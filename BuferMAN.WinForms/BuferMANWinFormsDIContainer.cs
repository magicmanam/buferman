using BuferMAN.Form;
using BuferMAN.Infrastructure;
using BuferMAN.Windows;
using SimpleInjector;

namespace BuferMAN.WinForms
{
    public class BufermanWinFormsDIContainer : BufermanWindowsDIContainer
    {
        public BufermanWinFormsDIContainer()
        {
            this.Register<IBuferMANHost, BuferAMForm>(Lifestyle.Singleton);
        }
    }
}
