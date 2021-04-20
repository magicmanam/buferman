using BuferMAN.Infrastructure.Settings;
using SimpleInjector;

namespace BuferMAN.Settings
{
    public static class ContainerExtensions
    {
        public static Container RegisterSettingsPart(this Container container)
        {
            container.Register<IProgramSettings, ProgramSettings>(Lifestyle.Singleton);

            return container;
        }
    }
}
