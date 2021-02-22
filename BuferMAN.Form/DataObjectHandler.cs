using BuferMAN.View;
using System.Linq;
using System.Windows.Forms;
using BuferMAN.Infrastructure;
using BuferMAN.Clipboard;
using BuferMAN.Form.Properties;
using magicmanam.UndoRedo;
using System;

namespace BuferMAN.Form
{
	class DataObjectHandler : IIDataObjectHandler
    {
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly BuferAMForm _form;

        public event EventHandler Updated;

        public DataObjectHandler(IClipboardBuferService clipboardBuferService, BuferAMForm form)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._form = form;
        }

        public void HandleDataObject(BuferViewModel buferViewModel)
        {
            var dataObject = buferViewModel.Clip;
            if (buferViewModel.Persistent || !this._clipboardBuferService.IsLastTemporaryClip(dataObject)) // Repeated Ctrl + C operation on the save object
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
                        if (this._clipboardBuferService.GetPersistentClips().Count() == BuferAMForm.MAX_BUFERS_COUNT)
                        {
                            MessageBox.Show(Resource.AllBufersPersistent, Resource.TratataTitle);
                            // Maybe display a program window if not ?
                            // Maybe all visible bufers can not be persistent (create a limit of persistent bufers)?
                            return;
                        }
                        else
                        {
                            if (this._clipboardBuferService.ClipsCount == BuferAMForm.MAX_BUFERS_COUNT + BuferAMForm.EXTRA_BUFERS_COUNT)
                            {
                                this._clipboardBuferService.RemoveClip(this._clipboardBuferService.FirstTemporaryClip);
                            }

                            this._clipboardBuferService.AddTemporaryClip(dataObject);
                            if (buferViewModel.Persistent)
                            {
                                this._clipboardBuferService.TryMarkClipAsPersistent(dataObject);
                            }
                        }
                    }
                }

                this.Updated?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
