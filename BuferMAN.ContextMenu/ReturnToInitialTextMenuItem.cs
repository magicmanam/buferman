using BuferMAN.ContextMenu.Properties;
using System;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class ReturnToInitialTextMenuItem : ChangingTextMenuItemBase
    {
        public ReturnToInitialTextMenuItem(Button button, string originBuferText, ToolTip mouseOverTooltip) : base(button, originBuferText, mouseOverTooltip)
        {
            this.Text = Resource.MenuReturn;
            this.Click += this._ReturnTextToInitial;
            this.Shortcut = Shortcut.CtrlI;
            this.Enabled = false;
        }

        private void _ReturnTextToInitial(object sender, EventArgs e)
        {
            this.TryChangeText(this.OriginBuferText);
            this.Enabled = false;
        }
    }
}
