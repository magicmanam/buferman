using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using SimpleInjector;

namespace BuferMAN.ContextMenu
{
    public static class ContainerExtensions
    {
        public static Container RegisterContextMenuPart(this Container container)
        {
            container.Register<IBuferContextMenuGenerator, BuferContextMenuGenerator>(Lifestyle.Singleton);
            container.Register<IBuferSelectionHandlerFactory, BuferSelectionHandlerFactory>(Lifestyle.Singleton);

            return container;
        }
    }
}
