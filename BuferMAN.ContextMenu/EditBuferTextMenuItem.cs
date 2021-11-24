using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.View;
using System;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class EditBuferTextMenuItem
    {
        private readonly IProgramSettingsGetter _settings;
        private readonly IBufermanHost _bufermanHost;
        private readonly IClipboardWrapper _clipboardWrapper;
        private readonly BuferContextMenuState _buferContextMenuState;

        public EditBuferTextMenuItem(
            BufermanMenuItem menuItem,
            BuferContextMenuState buferContextMenuState,
            IBufermanHost bufermanHost,
            IProgramSettingsGetter settings,
            IClipboardWrapper clipboardWrapper)
        {
            this._settings = settings;
            this._bufermanHost = bufermanHost;
            this._buferContextMenuState = buferContextMenuState;
            this._clipboardWrapper = clipboardWrapper;

            menuItem.AddOnClickHandler(this._ChangeBufer);
            menuItem.ShortCut = Shortcut.CtrlE;
        }

        private void _ChangeBufer(object sender, EventArgs e)
        {
            var buferText = this._buferContextMenuState.Bufer.ViewModel.TextData;
            var promptText = buferText.Length < this._settings.MaxBuferLengthToShowOnAliasCreation ?
                string.Format(Resource.ChangeText, buferText) :
                Resource.ChangeBigText;

            var newBuferText = this._bufermanHost.UserInteraction.PromptPopup(promptText,
                   Resource.ChangeTextTitle,
                   buferText);

            if (newBuferText != buferText)
            {
                var newClip = new DataObject();
                newClip.SetText(newBuferText);

                this._clipboardWrapper.SetDataObject(newClip);
                this._buferContextMenuState.RemoveBufer();
            }
        }
    }
}
