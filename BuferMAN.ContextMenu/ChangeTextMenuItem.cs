using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.View;
using System;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class ChangeTextMenuItem : ChangingTextMenuItemBase
    {
        public ChangeTextMenuItem(BufermanMenuItem menuItem, Button button, ToolTip mouseOverTooltip, IBufermanHost bufermanHost) : base(menuItem, button, mouseOverTooltip, bufermanHost)
        {
            menuItem.AddOnClickHandler(this._ChangeText);
            menuItem.ShortCut = Shortcut.CtrlH;
        }

        private void _ChangeText(object sender, EventArgs e)
        {
            var buferText = (this.Button.Tag as BuferViewModel).OriginBuferText;
            var promptText = buferText.Length < 100 ? // TODO (s) into settings? Just consider
                string.Format(Resource.ChangeText, buferText) :
                Resource.ChangeBigText;

            var newText = this.BufermanHost.UserInteraction.PromptPopup(promptText,
                   Resource.ChangeTextTitle,
                   this.Button.Text);

            this.TryChangeText(newText);
        }
    }
}
