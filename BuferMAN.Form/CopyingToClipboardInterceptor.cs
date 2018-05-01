using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BuferMAN.Infrastructure;
using BuferMAN.Clipboard;
using BuferMAN.Form.Properties;

namespace BuferMAN.Form
{
	class CopyingToClipboardInterceptor : ICopyingToClipboardInterceptor
    {
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly IEqualityComparer<IDataObject> _comparer;
		private readonly BuferAMForm _form;
        private readonly IClipboardWrapper _clipboardWrapper;

        public CopyingToClipboardInterceptor(IClipboardBuferService clipboardBuferService, BuferAMForm form, IEqualityComparer<IDataObject> comparer, IClipboardWrapper clipboardWrapper)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._form = form;
			this._comparer = comparer;
            this._clipboardWrapper = clipboardWrapper;
        }

        public void DoOnCtrlC()
        {
            //Here I need undoable context
            var currentObject = this._clipboardWrapper.GetDataObject();

            if (currentObject.GetFormats().Any() && !this._clipboardBuferService.IsLastTemporaryClip(currentObject))
            {
				if (this._clipboardBuferService.Contains(currentObject))
                {
					if (this._clipboardBuferService.IsNotPersistent(currentObject))
					{
						_clipboardBuferService.RemoveClip(currentObject);
					} else
					{
						return;
					}
                }

                if (this._clipboardBuferService.ClipsCount == BuferAMForm.MAX_BUFERS_COUNT)
                {
                    if (this._clipboardBuferService.GetTemporaryClips().Any())
                    {
                        this._clipboardBuferService.RemoveClip(this._clipboardBuferService.FirstTemporaryClip);
                    } else
                    {
                        MessageBox.Show(Resource.AllBufersPersistent, Resource.TratataTitle);
                        return;
                    }
                }

                this._clipboardBuferService.AddTemporaryClip(currentObject);

                if (this._form.WindowState != FormWindowState.Minimized && this._form.Visible)
                {
                    WindowLevelContext.Current.RerenderBufers();
                }
            }
        }
    }
}
