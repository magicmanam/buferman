using System;
using System.Globalization;
using System.Windows.Forms;

namespace ClipboardViewer
{
	class ClipSelectionHandler
    {
        private readonly WindowHidingHandler _hidingHandler;
        private readonly IDataObject _dataObject;

        public ClipSelectionHandler(Form form, IDataObject dataObject)
        {
            _hidingHandler = new WindowHidingHandler(form);
            this._dataObject = dataObject;
        }

        public void DoOnClipSelection(object sender, EventArgs e)
        {
			Logger.Logger.Current.Write("Do on clip selection");
			Clipboard.SetDataObject(this._dataObject);

            this._hidingHandler.HideWindow();

            var currentLanguage = InputLanguage.CurrentInputLanguage;
            InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new CultureInfo("en-US"));//This culture should be calculated automatically
            SendKeys.Send("^(v)");
            InputLanguage.CurrentInputLanguage = currentLanguage;
        }
    }
}