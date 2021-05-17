using BuferMAN.View;
using System.Linq;
using BuferMAN.Infrastructure;
using BuferMAN.Clipboard;
using magicmanam.UndoRedo;
using System;
using BuferMAN.Infrastructure.Settings;
using System.Windows.Forms;

namespace BuferMAN.Application
{
	internal class DataObjectHandler : IIDataObjectHandler
    {
        private readonly IClipboardBuferService _clipboardBuferService;
		private readonly IProgramSettingsGetter _settings;

        public event EventHandler<ClipboardUpdatedEventArgs> Updated;
        public event EventHandler Full;

        public DataObjectHandler(IClipboardBuferService clipboardBuferService, IProgramSettingsGetter settings)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._settings = settings;
        }

        private long _copiesCount = 0;
        private long _currentDayCopiesCount = 0;
        private DateTime _latestCopyDay = DateTime.Now.Date;
        
        private void _IncrementCopiesCounters()
        {
            this._copiesCount++;

            if (DateTime.Now.Date != this._latestCopyDay)
            {
                this._latestCopyDay = DateTime.Now.Date;
                this._currentDayCopiesCount = 0;
            }

            this._currentDayCopiesCount++;
        }

        public long CopiesCount { get { return this._copiesCount; } }
        public long CurrentDayCopiesCount { get { return this._currentDayCopiesCount; } }

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
            this._IncrementCopiesCounters();

            if (buferViewModel.Clip.GetData(DataFormats.StringFormat) as string == string.Empty)
            {
                // TODO (s) maybe set System.String data ? Can be implemented via setting. Such bufers can be marked
            }

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
