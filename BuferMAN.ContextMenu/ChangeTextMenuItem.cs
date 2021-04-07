using BuferMAN.ContextMenu.Properties;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.View;
using Microsoft.VisualBasic;
using System;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class ChangeTextMenuItem : ChangingTextMenuItemBase
    {
        public ChangeTextMenuItem(BufermanMenuItem menuItem, Button button, ToolTip mouseOverTooltip, IBufermanHost bufermanHost) : base(menuItem, button, mouseOverTooltip, bufermanHost)
        {
            menuItem.SetOnClickHandler(this._ChangeText);
            menuItem.ShortCut = Shortcut.CtrlH;
        }

        private void _ChangeText(object sender, EventArgs e)
        {
            var newText = Interaction.InputBox(Resource.ChangeTextPrefix + $" \"{(this.Button.Tag as BuferViewModel).OriginBuferText}\". " + Resource.ChangeTextPostfix,
                   Resource.ChangeTextTitle,
                   this.Button.Text);

            this.TryChangeText(newText);
        }
    }
}
