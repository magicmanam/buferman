using ClipboardViewerForm.Window;
using System;
using System.Globalization;
using System.Windows.Forms;
using Logging;
using ClipboardBufer;

namespace ClipboardViewerForm
{
	class BuferSelectionHandler : IBuferSelectionHandler
    {
        private readonly IWindowHidingHandler _hidingHandler;
        private readonly IDataObject _dataObject;
        private readonly IClipboardWrapper _clipboardWrapper;

        public BuferSelectionHandler(Form form, IDataObject dataObject, IWindowHidingHandler hidingHandler, IClipboardWrapper clipboardWrapper)
        {
            this._hidingHandler = hidingHandler;
            this._dataObject = dataObject;
            this._clipboardWrapper = clipboardWrapper;
        }

        public void DoOnClipSelection(object sender, EventArgs e)
        {
			this._clipboardWrapper.SetDataObject(this._dataObject);

            this._hidingHandler.HideWindow();

            var currentLanguage = InputLanguage.CurrentInputLanguage;
            InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new CultureInfo("en-US"));//This culture should be calculated automatically
            SendKeys.Send("^(v)");
            InputLanguage.CurrentInputLanguage = currentLanguage;
        }
    }
}