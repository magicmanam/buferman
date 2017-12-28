using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardViewerForm.ClipMenu
{
    interface IClipMenuGenerator
    {
        ContextMenu GenerateContextMenu(IDataObject dataObject, Button button, String originBuferText, string tooltipText, ToolTip mouseOverTooltip, bool isChangeTextAvailable);
    }
}
