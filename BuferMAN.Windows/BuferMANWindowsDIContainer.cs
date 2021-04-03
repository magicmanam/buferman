using BuferMAN.DI;
using BuferMAN.Infrastructure.Environment;
using SimpleInjector;

namespace BuferMAN.Windows
{
    public class BufermanWindowsDIContainer : BufermanDIContainer
    {
        public BufermanWindowsDIContainer()
        {
            this.Register<IStarter, Starter>(Lifestyle.Singleton);
            this.Register<IUserInteraction, UserInteraction>(Lifestyle.Singleton);
        }
    }
}
