﻿using BuferMAN.View;
using System.Linq;
using BuferMAN.Infrastructure;
using BuferMAN.Clipboard;
using magicmanam.UndoRedo;
using System;

namespace BuferMAN.Form
{
	public class DataObjectHandler : IIDataObjectHandler
    {
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly IProgramSettings _settings;

        public event EventHandler<ClipboardUpdatedEventArgs> Updated;
        public event EventHandler Full;

        public DataObjectHandler(IClipboardBuferService clipboardBuferService, IProgramSettings settings)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._settings = settings;
        }

        public long CopiesCount { get; private set; } = 0;

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
            this.CopiesCount++;

            var isLastTempBufer = this._clipboardBuferService.IsLastTemporaryBufer(buferViewModel);

            if (!buferViewModel.Pinned && isLastTempBufer) // Repeated Ctrl + C operation
            {
                buferViewModel.ViewId = this._clipboardBuferService.LastTemporaryBufer.ViewId;
                this.Updated?.Invoke(this, new ClipboardUpdatedEventArgs(buferViewModel));
                return false;
            }
            if (this._clipboardBuferService.IsInPinnedBufers(buferViewModel, out Guid pinnedBuferViewId))
            {
                buferViewModel.ViewId = pinnedBuferViewId;
                this.Updated?.Invoke(this, new ClipboardUpdatedEventArgs(buferViewModel));
                return false;
            }

            var alreadyInTempBufers = this._clipboardBuferService.IsInTemporaryBufers(buferViewModel, out Guid viewId);

            if (!alreadyInTempBufers && this._clipboardBuferService.GetPinnedBufers().Count() == this._settings.MaxBufersCount)
            {   // Maybe we should not do any check if persistent clips count = max bufers count
                // Maybe all visible bufers can not be persistent (create a limit of persistent bufers)?
                this.Full?.Invoke(this, EventArgs.Empty);
                this.Updated?.Invoke(this, new ClipboardUpdatedEventArgs(buferViewModel));
                return false;
            }

            using (UndoableContext<ApplicationStateSnapshot>.Current.StartAction())
            {
                if (alreadyInTempBufers)
                {
                    this._clipboardBuferService.RemoveBufer(viewId);
                }
                else if (this._clipboardBuferService.BufersCount == this._settings.MaxBufersCount + this._settings.ExtraBufersCount)
                {
                    this._clipboardBuferService.RemoveBufer(this._clipboardBuferService.FirstTemporaryBufer.ViewId);
                }

                buferViewModel.ViewId = Guid.NewGuid();
                this._clipboardBuferService.AddTemporaryClip(buferViewModel);

                if (buferViewModel.Pinned)
                {
                    this._clipboardBuferService.TryPinBufer(buferViewModel.ViewId);
                }
            }

            this.Updated?.Invoke(this, new ClipboardUpdatedEventArgs(buferViewModel));
            return true;
        }
    }
}
