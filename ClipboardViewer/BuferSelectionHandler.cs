using System;
using System.Globalization;
using System.Windows.Forms;

namespace ClipboardViewer
{
	class BuferSelectionHandler
    {
        private readonly IWindowHidingHandler _hidingHandler;
        private readonly IDataObject _dataObject;

        public BuferSelectionHandler(Form form, IDataObject dataObject, IWindowHidingHandler hidingHandler)
        {
            _hidingHandler = hidingHandler;
            this._dataObject = dataObject;
        }

        public void DoOnClipSelection(object sender, EventArgs e)
        {
			Logger.Logger.Current.Write("Do on bufer selection");
			Clipboard.SetDataObject(this._dataObject);

            this._hidingHandler.HideWindow();

            var currentLanguage = InputLanguage.CurrentInputLanguage;
            InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new CultureInfo("en-US"));//This culture should be calculated automatically
            SendKeys.Send("^(v)");
            InputLanguage.CurrentInputLanguage = currentLanguage;
        }
    }
}