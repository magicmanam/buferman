using BuferMAN.Infrastructure.Menu;
using SimpleInjector;

namespace BuferMAN.Menu
{
    public static class ContainerExtensions
    {
        public static Container RegisterMainMenuPart(this Container container)
        {
            container.Register<IMainMenuGenerator, MainMenuGenerator>(Lifestyle.Singleton);

            return container;
        }
    }
}
