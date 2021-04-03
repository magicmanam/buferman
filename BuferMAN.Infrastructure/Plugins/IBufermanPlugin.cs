using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.Infrastructure.Plugins
{
    public interface IBufermanPlugin
    {
        void InitializeMainMenu(BuferMANMenuItem menuItem);
        void InitializeHost(IBufermanHost bufermanHost);
    }
}
