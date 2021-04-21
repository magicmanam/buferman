using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using System;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class ReturnToInitialTextMenuItem : ChangingTextMenuItemBase
    {
        public ReturnToInitialTextMenuItem(BufermanMenuItem menuItem, IBufer bufer, IBufermanHost bufermanHost) : base(menuItem, bufer, bufermanHost)
        {
            menuItem.AddOnClickHandler(this._ReturnTextToInitial);
            menuItem.ShortCut = Shortcut.CtrlI;
            menuItem.Enabled = false;
        }

        private void _ReturnTextToInitial(object sender, EventArgs e)
        {
            this.TryChangeText(this.Bufer.ViewModel.OriginBuferText);
            this.MenuItem.Enabled = false;
        }
    }
}
