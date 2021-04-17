using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.View;
using System;
using System.Windows.Forms;

namespace BuferMAN.ContextMenu
{
    public class BuferContextMenuModelWrapper : BuferContextMenuModel
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IBuferSelectionHandler _buferSelectionHandler;
        private readonly IBufermanHost _bufermanHost;

        public BuferViewModel BuferViewModel;
        public Button Button;
        public BufermanMenuItem ReturnTextToInitialMenuItem;
        public BufermanMenuItem ChangeTextMenuItem;
        public BufermanMenuItem MarkAsPinnedMenuItem;
        public BufermanMenuItem CreateLoginDataMenuItem;
        public BufermanMenuItem AddToFileMenuItem;
        public BufermanMenuItem PasteMenuItem;
        public BufermanMenuItem PlaceInBuferMenuItem;
        public DeleteClipMenuItem DeleteMenuItem { get; set; }
        public ToolTip MouseOverTooltip;

        public BuferContextMenuModelWrapper(IClipboardBuferService clipboardBuferService, IBuferSelectionHandler buferSelectionHandler, IBufermanHost bufermanHost)
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
            this.BuferViewModel.Clip.SetData(ClipboardFormats.PASSWORD_FORMAT, e.Password);
            if (!this.BuferViewModel.Pinned)
            {
                this.TryTogglePinBufer(sender, e);
            }

            this.Button.Click -= this._buferSelectionHandler.DoOnClipSelection;
        }

        public void TryTogglePinBufer(object sender, EventArgs e)
        {
            if (this.BuferViewModel.Pinned)
            {
                if (this._clipboardBuferService.TryUnpinBufer(this.BuferViewModel.ViewId))
                {
                    this.BuferViewModel.Pinned = false;
                    this.MarkAsPinnedMenuItem.Text = Resource.MenuPin;
                    this._bufermanHost.RerenderBufers();
                }
            }
            else
            {
                if (this._clipboardBuferService.TryPinBufer(this.BuferViewModel.ViewId))
                {
                    this.BuferViewModel.Pinned = true;
                    this.MarkAsPinnedMenuItem.Text = Resource.MenuUnpin;
                    this._bufermanHost.RerenderBufers();

                    if (this.DeleteMenuItem.IsDeferredDeletionActivated())
                    {
                        this.DeleteMenuItem.CancelDeferredBuferDeletion(sender, e);
                    }
                }
            }
        }
    }
}
