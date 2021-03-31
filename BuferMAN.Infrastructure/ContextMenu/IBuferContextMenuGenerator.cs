using BuferMAN.View;
using System;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure.ContextMenu
{
    public interface IBuferContextMenuGenerator
    {
        System.Windows.Forms.ContextMenu GenerateContextMenu(BuferViewModel buferViewModel, Button button, ToolTip mouseOverTooltip, bool isChangeTextAvailable, IBuferSelectionHandler buferSelectionHandler, IBuferMANHost buferMANHost);
    }
}
