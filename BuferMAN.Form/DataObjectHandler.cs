using BuferMAN.View;
using System.Linq;
using BuferMAN.Infrastructure;
using BuferMAN.Clipboard;
using magicmanam.UndoRedo;
using System;

namespace BuferMAN.Form
{
	class DataObjectHandler : IIDataObjectHandler
    {
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly BuferAMForm _form;

        public event EventHandler Updated;
        public event EventHandler Full;

        public DataObjectHandler(IClipboardBuferService clipboardBuferService, BuferAMForm form)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._form = form;
        }

        // For unit tests:
        // not persistent: last temp ? <nothing> :
        // any temp ? <swap> : 
        // pinned ? <nothing> :
        // (not exist) - <add temp>

        // persistent: pinned ? <nothing> :
        // any temp ? <remove from temp and add pinned> : 
        // (not exist) <add pinned>
        public bool TryHandleDataObject(BuferViewModel buferViewModel)
        {
            var isLastTempBufer = this._clipboardBuferService.IsLastTemporaryBufer(buferViewModel);

            if (!buferViewModel.Persistent && isLastTempBufer // Repeated Ctrl + C operation
                || this._clipboardBuferService.IsPersistent(buferViewModel.Clip))
            {
                return false;
            }

            var alreadyInTempBufers = this._clipboardBuferService.IsInTemporaryBufers(buferViewModel);

            if (!alreadyInTempBufers && this._clipboardBuferService.GetPersistentClips().Count() == BuferAMForm.MAX_BUFERS_COUNT)
            {   // Maybe we should not do any check if persistent clips count = max bufers count
                // Maybe all visible bufers can not be persistent (create a limit of persistent bufers)?
                this.Full?.Invoke(this, EventArgs.Empty);
                return false;
            }

            using (UndoableContext<ClipboardBuferServiceState>.Current.StartAction())
            {
                if (!alreadyInTempBufers && this._clipboardBuferService.ClipsCount == BuferAMForm.MAX_BUFERS_COUNT + BuferAMForm.EXTRA_BUFERS_COUNT)
                {
                    this._clipboardBuferService.RemoveClip(this._clipboardBuferService.FirstTemporaryClip);
                }

                var dataObject = buferViewModel.Clip;

                if (alreadyInTempBufers)
                {
                    this._clipboardBuferService.RemoveClip(dataObject);
                }

                this._clipboardBuferService.AddTemporaryClip(dataObject);

                if (buferViewModel.Persistent)
                {
                    this._clipboardBuferService.TryMarkClipAsPersistent(dataObject);
                }
            }

            this.Updated?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}
