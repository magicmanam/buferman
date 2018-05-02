using System;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure.ContextMenu
{
    public interface IClipMenuGenerator
    {
        System.Windows.Forms.ContextMenu GenerateContextMenu(IDataObject dataObject, Button button, ToolTip mouseOverTooltip, bool isChangeTextAvailable);
    }
}
