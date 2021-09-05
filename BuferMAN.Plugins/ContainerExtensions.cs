using BuferMAN.Infrastructure.Plugins;
using SimpleInjector;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BuferMAN.Plugins
{
    public static class ContainerExtensions
    {
        public static Container RegisterPlugins(this Container container)
        {
            var pluginDirectory = AppDomain.CurrentDomain.BaseDirectory;

            if (Directory.Exists(pluginDirectory))
            {
                var pluginAssemblies =
                from file in new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).GetFiles()
                where file.FullName.EndsWith(".Plugin.dll") || file.FullName.EndsWith(".Plugins.dll")
                select Assembly.Load(AssemblyName.GetAssemblyName(file.FullName));

                container.Collection.Register<IBufermanPlugin>(pluginAssemblies, Lifestyle.Singleton);
            }

            return container;
        }
    }
}
