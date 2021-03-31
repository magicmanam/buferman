using BuferMAN.Infrastructure.Menu;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure
{
    public interface IBufer
    {
        void SetContextMenu(IEnumerable<BuferMANMenuItem> menuItems);
    }
}
