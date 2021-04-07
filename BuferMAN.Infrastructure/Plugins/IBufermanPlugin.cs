using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.Infrastructure.Plugins
{
    public interface IBufermanPlugin
    {
        void InitializeMainMenu(BufermanMenuItem menuItem);
        void InitializeHost(IBufermanHost bufermanHost);
        string Name { get; }
    }
}
