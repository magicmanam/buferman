using BuferMAN.Infrastructure.Files;
using SimpleInjector;

namespace BuferMAN.Files
{
    public static class ContainerExtensions
    {
        public static Container RegisterFilesPart(this Container container)
        {
            container.Register<IUserFileSelector, UserFileStorageSelector>(Lifestyle.Singleton);
            container.Register<IFileStorage, FileStorage>(Lifestyle.Singleton);
            container.Register<IBufersFileStorageFactory, BufersFileStorageFactory>(Lifestyle.Singleton);

            return container;
        }
    }
}
