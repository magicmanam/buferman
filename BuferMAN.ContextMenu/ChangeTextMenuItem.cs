using BuferMAN.ContextMenu.Properties;
using BuferMAN.View;
using Microsoft.VisualBasic;
using System;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class ChangeTextMenuItem : ChangingTextMenuItemBase
    {
        public ChangeTextMenuItem(Button button, ToolTip mouseOverTooltip) : base(button, mouseOverTooltip)
        {
            this.Text = Resource.MenuChange;
            this.Click += this._ChangeText;
            this.Shortcut = Shortcut.CtrlH;
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
