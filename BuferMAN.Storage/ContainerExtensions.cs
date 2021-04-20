using BuferMAN.Infrastructure.Storage;
using SimpleInjector;

namespace BuferMAN.Storage
{
    public static class ContainerExtensions
    {
        public static Container RegisterStoragePart(this Container container)
        {
            container.Register<IBuferItemDataObjectConverter, BuferItemDataObjectConverter>(Lifestyle.Singleton);
            container.Register<ILoadedBuferItemsProcessor, LoadedBuferItemsProcessor>(Lifestyle.Singleton);
            container.Register<IBufersStorageFactory, BufersStorageFactory>(Lifestyle.Singleton);

            return container;
        }
    }
}
