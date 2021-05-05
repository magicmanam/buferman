using BuferMAN.Infrastructure;
using SimpleInjector;

namespace BuferMAN.Application
{
    public static class ContainerExtensions
    {
        public static Container RegisterApplicationPart(this Container container)
        {
            container.Register<IBufermanApplication, BufermanApplication>(Lifestyle.Singleton);
            container.Register<IIDataObjectHandler, DataObjectHandler>(Lifestyle.Singleton);
            container.Register<IWindowLevelContext, DefaultWindowLevelContext>(Lifestyle.Singleton);

            container.Register<IMapper, Mapper>(Lifestyle.Singleton);

            return container;
        }
    }
}
