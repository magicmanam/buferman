using BuferMAN.Clipboard;
using BuferMAN.Infrastructure.Menu;
using System;

namespace BuferMAN.Infrastructure.ContextMenu
{
    public class BuferContextMenuState
    {// TODO (m) update this class with properties from BuferContextMenuModelWrapper class
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IBufermanHost _bufermanHost;

        public BuferContextMenuState(IClipboardBuferService clipboardBuferService, IBufermanHost bufermanHost)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._bufermanHost = bufermanHost;
        }

        public IBufer Bufer { get; set; }

        public event EventHandler<BuferPinnedEventArgs> PinnedBufer;
        public BufermanMenuItem DeleteBuferMenuItem { get; set; }

        protected void InvokePinnedBuferEvent(BuferPinnedEventArgs args)
        {
            this.PinnedBufer?.Invoke(this, args);
        }

        public void RemoveBufer()
        {
            this._clipboardBuferService.RemoveBufer(this.Bufer.ViewModel.ViewId);
            this._bufermanHost.RerenderBufers();
        }
    }
}