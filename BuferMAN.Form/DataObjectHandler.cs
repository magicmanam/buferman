using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BuferMAN.Infrastructure;
using BuferMAN.Clipboard;
using BuferMAN.Form.Properties;
using magicmanam.UndoRedo;

namespace BuferMAN.Form
{
	class DataObjectHandler : IIDataObjectHandler
    {
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly IEqualityComparer<IDataObject> _comparer;
		private readonly BuferAMForm _form;

        public DataObjectHandler(IClipboardBuferService clipboardBuferService, BuferAMForm form, IEqualityComparer<IDataObject> comparer)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._form = form;
			this._comparer = comparer;
        }

        public void HandleDataObject(IDataObject dataObject)
        {
            if (dataObject.GetFormats().Any() && !this._clipboardBuferService.IsLastTemporaryClip(dataObject))
            {
                using (UndoableContext<ClipboardBuferServiceState>.Current.StartAction())
                {
                    if (this._clipboardBuferService.Contains(dataObject))
                    {
                        if (this._clipboardBuferService.IsPersistent(dataObject))
                        {
                            return;                           
                        }
                        else
                        {
                            this._clipboardBuferService.RemoveClip(dataObject);
                            this._clipboardBuferService.AddTemporaryClip(dataObject);// Maybe just swap these clips? But they can be different.
                        }
                    }
                    else
                    {
                        if (this._clipboardBuferService.ClipsCount == BuferAMForm.MAX_BUFERS_COUNT)
                        {
                            if (this._clipboardBuferService.GetTemporaryClips().Any())
                            {
                                this._clipboardBuferService.RemoveClip(this._clipboardBuferService.FirstTemporaryClip);
                                this._clipboardBuferService.AddTemporaryClip(dataObject);
                            }
                            else
                            {
                                MessageBox.Show(Resource.AllBufersPersistent, Resource.TratataTitle);
                                return;
                            }
                        }
                        else
                        {
                            this._clipboardBuferService.AddTemporaryClip(dataObject);
                        }
                    }
                }

                if (this._form.WindowState != FormWindowState.Minimized && this._form.Visible)
                {
                    WindowLevelContext.Current.RerenderBufers();
                }
            }
        }
    }
}
