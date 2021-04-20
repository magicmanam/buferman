using BuferMAN.Infrastructure.Plugins;
using SimpleInjector;

namespace BuferMAN.Plugins
{
    public static class ContainerExtensions
    {
        public static Container RegisterPlugins(this Container container)
        {
            container.Collection.Register<IBufermanPlugin>(new[] { typeof(BufermanPluginBase).Assembly }, Lifestyle.Singleton);

            return container;
        }
    }
}
