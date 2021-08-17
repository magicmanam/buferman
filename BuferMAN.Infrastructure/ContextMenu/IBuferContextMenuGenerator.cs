using BuferMAN.Infrastructure.Menu;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure.ContextMenu
{
    public interface IBuferContextMenuGenerator
    {
        IEnumerable<BufermanMenuItem> GenerateContextMenuItems(BuferContextMenuState buferContextMenuState,
            IBufermanHost bufermanHost,
            IBuferTypeMenuGenerator buferTypeGenerator);
    }
}
