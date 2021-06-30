using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using System;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class ChangeTextMenuItem : ChangingTextMenuItemBase
    {
        public ChangeTextMenuItem(BufermanMenuItem menuItem, IBufer bufer, IBufermanHost bufermanHost) : base(menuItem, bufer, bufermanHost)
        {
            menuItem.AddOnClickHandler(this._ChangeText);
            menuItem.ShortCut = Shortcut.CtrlH;
        }

        private void _ChangeText(object sender, EventArgs e)
        {
            var buferText = this.ViewModel.OriginBuferTitle;
            var promptText = buferText.Length < 100 ? // TODO (s) into settings? Just consider
                string.Format(Resource.ChangeText, buferText) :
                Resource.ChangeBigText;

            var newText = this.BufermanHost.UserInteraction.PromptPopup(promptText,
                   Resource.ChangeTextTitle,
                   this.ViewModel.Alias ?? this.ViewModel.OriginBuferTitle);

            this.TryChangeText(newText);
        }
    }
}
