using BuferMAN.ContextMenu.Properties;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class MakePinnedMenuItem : MenuItem
    {
        public MakePinnedMenuItem()
        {
            this.Text = Resource.MenuPin;
            this.Shortcut = Shortcut.CtrlS;
        }
    }
}
