using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using System;
using BuferMAN.Infrastructure.ContextMenu;

namespace BuferMAN.ContextMenu
{
    internal class BuferContextMenuStateWrapper : BuferContextMenuState
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IBuferSelectionHandler _buferSelectionHandler;
        private readonly IBufermanHost _bufermanHost;

        public BufermanMenuItem ReturnTextToInitialMenuItem;
        public BufermanMenuItem ChangeTextMenuItem;
        public BufermanMenuItem MarkAsPinnedMenuItem;
        public BufermanMenuItem CreateLoginDataMenuItem;
        public BufermanMenuItem AddToFileMenuItem;
        public BufermanMenuItem PasteMenuItem;
        public BufermanMenuItem PlaceInBuferMenuItem;

        public BuferContextMenuStateWrapper(IClipboardBuferService clipboardBuferService, IBuferSelectionHandler buferSelectionHandler, IBufermanHost bufermanHost)
            : base(clipboardBuferService, bufermanHost)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._buferSelectionHandler = buferSelectionHandler;
            this._bufermanHost = bufermanHost;
        }

        public void ChangeTextMenuItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.ReturnTextToInitialMenuItem.Enabled = !e.IsOriginText;
        }

        public void MarkMenuItemAsAddedToFile()
        {
            this.AddToFileMenuItem.Text = Resource.MenuAddedToFile;
            this.AddToFileMenuItem.Enabled = false;
        }

        public void LoginCredentialsMenuItem_LoginCreated(object sender, CreateLoginCredentialsEventArgs e)
        {
            this.PasteMenuItem.Text = $"{Resource.LoginWith} {new String('\t', 2)} Enter";

            this.ReturnTextToInitialMenuItem.Enabled = false;
            this.PlaceInBuferMenuItem.Enabled = false;
            this.ChangeTextMenuItem.Enabled = false;
            this.Bufer.ViewModel.Clip.SetData(ClipboardFormats.PASSWORD_FORMAT, e.Password);
            if (!this.Bufer.ViewModel.Pinned)
            {
                this.TryTogglePinBufer(sender, e);
            }

            this.Bufer.RemoveOnClickHandler(this._buferSelectionHandler.DoOnClipSelection);
        }

        public void TryTogglePinBufer(object sender, EventArgs e)
        {
            if (this.Bufer.ViewModel.Pinned)
            {
                if (this._clipboardBuferService.TryUnpinBufer(this.Bufer.ViewModel.ViewId))
                {
                    this.Bufer.ViewModel.Pinned = false;
                    this.MarkAsPinnedMenuItem.Text = Resource.MenuPin;
                    this._bufermanHost.RerenderBufers();
                }
            }
            else
            {
                if (this._clipboardBuferService.TryPinBufer(this.Bufer.ViewModel.ViewId))
                {
                    this.Bufer.ViewModel.Pinned = true;
                    this.MarkAsPinnedMenuItem.Text = Resource.MenuUnpin;
                    this._bufermanHost.RerenderBufers();

                    this.InvokePinnedBuferEvent(new BuferPinnedEventArgs());
                }
            }
        }
    }
}
