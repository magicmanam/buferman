using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Settings;
using System;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class ChangeTextMenuItem : ChangingTextMenuItemBase
    {
        private readonly IProgramSettingsGetter _setting;

        public ChangeTextMenuItem(BufermanMenuItem menuItem, IBufer bufer, IBufermanHost bufermanHost, IProgramSettingsGetter setting) : base(menuItem, bufer, bufermanHost)
        {
            menuItem.AddOnClickHandler(this._ChangeText);
            menuItem.ShortCut = Shortcut.CtrlH;

            this._setting = setting;
        }

        private void _ChangeText(object sender, EventArgs e)
        {
            var buferText = this.ViewModel.OriginBuferTitle;
            var promptText = buferText.Length < this._setting.MaxBuferLengthToShowOnAliasCreation ?
                string.Format(Resource.ChangeText, buferText) :
                Resource.ChangeBigText;

            var newText = this.BufermanHost.UserInteraction.PromptPopup(promptText,
                   Resource.ChangeTextTitle,
                   this.ViewModel.Alias ?? this.ViewModel.OriginBuferTitle);

            this.TryChangeText(newText);
        }
    }
}
