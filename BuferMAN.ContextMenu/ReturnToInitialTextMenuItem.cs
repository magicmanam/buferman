using BuferMAN.ContextMenu.Properties;
using BuferMAN.View;
using System;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class ReturnToInitialTextMenuItem : ChangingTextMenuItemBase
    {
        public ReturnToInitialTextMenuItem(Button button, ToolTip mouseOverTooltip) : base(button, mouseOverTooltip)
        {
            this.Text = Resource.MenuReturn;
            this.Click += this._ReturnTextToInitial;
            this.Shortcut = Shortcut.CtrlI;
            this.Enabled = false;
        }

        private void _ReturnTextToInitial(object sender, EventArgs e)
        {
            this.TryChangeText((this.Button.Tag as BuferViewModel).OriginBuferText);
            this.Enabled = false;
        }
    }
}
