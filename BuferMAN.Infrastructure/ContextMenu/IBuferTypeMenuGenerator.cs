using BuferMAN.Infrastructure.Menu;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure.ContextMenu
{
    public interface IBuferTypeMenuGenerator
    {
        IList<BufermanMenuItem> Generate(IList<BufermanMenuItem> menuItems, IBufermanHost bufermanHost);
    }
}
