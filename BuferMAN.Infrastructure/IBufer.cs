using BuferMAN.Infrastructure.Menu;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure
{
    public interface IBufer
    {
        void SetContextMenu(IEnumerable<BuferMANMenuItem> menuItems);
        void SetButton(Button button);// TODO (l) remove this method
    }
}
