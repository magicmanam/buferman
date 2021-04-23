using BuferMAN.Models;
using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.Infrastructure.Plugins
{
    public interface IBufermanPlugin
    {
        BufermanMenuItem CreateMainMenuItem();
        void Initialize(IBufermanHost bufermanHost);
        BufermanMenuItem CreateBuferContextMenuItem();
        void UpdateBuferContextMenu(BuferContextMenuModel contextMenuModel);
        string Name { get; }
        bool Enabled { get; set; }
    }
}
