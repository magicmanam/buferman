using System;
using System.Globalization;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.Infrastructure;
using BuferMAN.Clipboard;
using SystemWindowsForm = System.Windows.Forms.Form;

namespace BuferMAN.ContextMenu
{
	public class BuferSelectionHandler : IBuferSelectionHandler
    {
        private readonly IDataObject _dataObject;
        private readonly IClipboardWrapper _clipboardWrapper;

        public BuferSelectionHandler(SystemWindowsForm form, IDataObject dataObject, IClipboardWrapper clipboardWrapper)
        {
            this._dataObject = dataObject;
            this._clipboardWrapper = clipboardWrapper;
        }

        public void DoOnClipSelection(object sender, EventArgs e)
        {
			this._clipboardWrapper.SetDataObject(this._dataObject);

            WindowLevelContext.Current.HideWindow();

            var currentLanguage = InputLanguage.CurrentInputLanguage;
            InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new CultureInfo("en-US"));//This culture should be calculated automatically

            new KeyboardEmulator()
                .HoldDownCtrl()
                .SendKeyboardKeys("v", false);

            InputLanguage.CurrentInputLanguage = currentLanguage;
        }
    }
}