using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.Infrastructure.Plugins
{
    public interface IBufermanPlugin
    {
        BufermanMenuItem CreateMainMenuItem();
        void Initialize(IBufermanHost bufermanHost);
        void UpdateBuferItem(BuferContextMenuState contextMenuModel);// TODO (s) rename BuferContextMenuState -> BuferContext
        string Name { get; }
        bool Enabled { get; set; }
        bool Available { get; }
    }
}
