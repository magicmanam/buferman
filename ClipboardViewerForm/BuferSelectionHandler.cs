using ClipboardViewerForm.Window;
using System;
using System.Globalization;
using System.Windows.Forms;
using Logging;
using ClipboardBufer;
using Windows;

namespace ClipboardViewerForm
{
	class BuferSelectionHandler : IBuferSelectionHandler
    {
        private readonly IDataObject _dataObject;
        private readonly IClipboardWrapper _clipboardWrapper;

        public BuferSelectionHandler(Form form, IDataObject dataObject, IClipboardWrapper clipboardWrapper)
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