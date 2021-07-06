using System;
using System.Globalization;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.Infrastructure;
using BuferMAN.Clipboard;

namespace BuferMAN.ContextMenu
{
	internal class BuferSelectionHandler : IBuferSelectionHandler
    {
        private readonly IDataObject _dataObject;
        private readonly IClipboardWrapper _clipboardWrapper;
        private readonly IBufermanHost _bufermanHost;

        public BuferSelectionHandler(IDataObject dataObject, IClipboardWrapper clipboardWrapper, IBufermanHost bufermanHost)
        {
            this._dataObject = dataObject;
            this._clipboardWrapper = clipboardWrapper;
            this._bufermanHost = bufermanHost;
        }

        public void DoOnClipSelection(object sender, EventArgs e)
        {
            this._clipboardWrapper.SetDataObject(this._dataObject);

            this._bufermanHost.HideWindow();

            var currentLanguage = InputLanguage.CurrentInputLanguage;
            InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new CultureInfo("en-US"));// TODO (m) This culture should be calculated automatically

            new KeyboardEmulator()
                .HoldDownCtrl()
                .SendKeyboardKeys("v", false);

            InputLanguage.CurrentInputLanguage = currentLanguage;
        }
    }
}