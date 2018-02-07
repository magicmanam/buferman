using ClipboardViewerForm.Properties;
using Microsoft.VisualBasic;
using System;
using System.Windows.Forms;

namespace ClipboardViewerForm.ClipMenu.Items
{
    class ChangeTextMenuItem : ChangingTextMenuItemBase
    {
        public ChangeTextMenuItem(Button button, string originBuferText, ToolTip mouseOverTooltip) : base(button, originBuferText, mouseOverTooltip)
        {
            this.Text = Resource.MenuChange;
            this.Click += this._ChangeText;
            this.Shortcut = Shortcut.CtrlH;
        }

        private void _ChangeText(object sender, EventArgs e)
        {
            var newText = Interaction.InputBox(Resource.ChangeTextPrefix + $" \"{this.OriginBuferText}\". " + Resource.ChangeTextPostfix,
                   Resource.ChangeTextTitle,
                   this.Button.Text);

            this.TryChangeText(newText);
        }
    }
}
