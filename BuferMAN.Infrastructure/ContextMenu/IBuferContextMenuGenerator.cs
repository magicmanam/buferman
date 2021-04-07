using BuferMAN.Infrastructure.Menu;
using BuferMAN.View;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure.ContextMenu
{
    public interface IBuferContextMenuGenerator
    {
        IEnumerable<BufermanMenuItem> GenerateContextMenu(BuferViewModel buferViewModel, Button button, ToolTip mouseOverTooltip, bool isChangeTextAvailable, IBuferSelectionHandler buferSelectionHandler, IBufermanHost bufermanHost);
    }
}
