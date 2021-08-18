using BuferMAN.Clipboard;
using BuferMAN.Infrastructure.Menu;
using System;

namespace BuferMAN.Infrastructure.ContextMenu
{
    public class BuferContextMenuState // TODO (m) replace this class from Infrastructure assembly + remove 'resource' lines from constructor
    {// TODO (m) update this class with properties from BuferContextMenuModelWrapper class
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IBuferSelectionHandler _buferSelectionHandler;
        private readonly IBufermanHost _bufermanHost;
        private readonly Func<string> _resourceMenuPin;
        private readonly Func<string> _resourceMenuUnpin;
        private readonly Func<string> _resourceMenuAddedToFile;

        public BufermanMenuItem ReturnTextToInitialMenuItem;
        public BufermanMenuItem ChangeTextMenuItem;
        public BufermanMenuItem MarkAsPinnedMenuItem;
        public BufermanMenuItem CreateLoginDataMenuItem;
        public BufermanMenuItem AddToFileMenuItem;
        public BufermanMenuItem PasteMenuItem;
        public BufermanMenuItem PlaceInBuferMenuItem;

        public BuferContextMenuState(
            IClipboardBuferService clipboardBuferService,
            IBuferSelectionHandler buferSelectionHandler,
            IBufermanHost bufermanHost,
            Func<string> resourceMenuPin,
            Func<string> resourceMenuUnpin,
            Func<string> resourceMenuAddedToFile,
            IBufer bufer)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._buferSelectionHandler = buferSelectionHandler;
            this._bufermanHost = bufermanHost;

            // TODO (m) remove below 3 lines after this class is relocated from infrastructure assembly
            this._resourceMenuPin = resourceMenuPin;
            this._resourceMenuUnpin = resourceMenuUnpin;
            this._resourceMenuAddedToFile = resourceMenuAddedToFile;

            this.Bufer = bufer;
            this.MarkAsPinnedMenuItem = bufermanHost.CreateMenuItem(this.Bufer.ViewModel.Pinned ? resourceMenuUnpin : resourceMenuPin, this.TryTogglePinBufer);
        }

        public IBufer Bufer { get; private set; }

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

        public void TryTogglePinBufer(object sender, EventArgs e)
        {
            if (this.Bufer.ViewModel.Pinned)
            {
                if (this._clipboardBuferService.TryUnpinBufer(this.Bufer.ViewModel.ViewId))
                {
                    this.Bufer.ViewModel.Pinned = false;
                    this.MarkAsPinnedMenuItem.Text = this._resourceMenuPin();
                    this._bufermanHost.RerenderBufers();
                }
            }
            else
            {
                if (this._clipboardBuferService.TryPinBufer(this.Bufer.ViewModel.ViewId))
                {
                    this.Bufer.ViewModel.Pinned = true;
                    this.MarkAsPinnedMenuItem.Text = this._resourceMenuUnpin();
                    this._bufermanHost.RerenderBufers();

                    this.InvokePinnedBuferEvent(new BuferPinnedEventArgs());
                }
            }
        }

        public void MarkMenuItemAsAddedToFile()
        {
            this.AddToFileMenuItem.Text = this._resourceMenuAddedToFile();
            this.AddToFileMenuItem.Enabled = false;
        }

        public void RemoveBuferSelectionHandler()
        {
            this.Bufer.RemoveOnClickHandler(this._buferSelectionHandler.DoOnClipSelection);
        }
    }
}