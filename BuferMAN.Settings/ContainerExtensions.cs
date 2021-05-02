using BuferMAN.Infrastructure.Settings;
using SimpleInjector;

namespace BuferMAN.Settings
{
    public static class ContainerExtensions
    {
        public static Container RegisterSettingsPart(this Container container)
        {
            var registration = Lifestyle.Singleton.CreateRegistration<ProgramSettings>(container);

            container.AddRegistration(typeof(IProgramSettingsGetter), registration);
            container.AddRegistration(typeof(IProgramSettingsSetter), registration);

            return container;
        }
    }
}
