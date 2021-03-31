using BuferMAN.Infrastructure.Menu;
using BuferMAN.View;
using System;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class ReturnToInitialTextMenuItem : ChangingTextMenuItemBase
    {
        public ReturnToInitialTextMenuItem(BuferMANMenuItem menuItem, Button button, ToolTip mouseOverTooltip) : base(menuItem, button, mouseOverTooltip)
        {
            menuItem.SetOnClickHandler(this._ReturnTextToInitial);
            menuItem.ShortCut = Shortcut.CtrlI;
            menuItem.Enabled = false;
        }

        private void _ReturnTextToInitial(object sender, EventArgs e)
        {
            this.TryChangeText((this.Button.Tag as BuferViewModel).OriginBuferText);
            this.MenuItem.Enabled = false;
        }
    }
}
