using BuferMAN.Clipboard;
using BuferMAN.ContextMenu.Properties;
using BuferMAN.Infrastructure;
using System;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.View;

namespace BuferMAN.ContextMenu
{
    public class DeleteClipMenuItem : MenuItem
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private BuferViewModel _bufer;
        private Button _button;

        public DeleteClipMenuItem(IClipboardBuferService clipboardBuferService, BuferViewModel bufer, Button button)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._bufer = bufer;
            this._button = button;
            this.Text = Resource.DeleteClipMenuItem;
            this.Shortcut = Shortcut.Del;
            this.Click += this._DeleteBufer;
        }

        private void _DeleteBufer(object sender, EventArgs e)
        {
            var tabIndex = this._button.TabIndex;
            this._clipboardBuferService.RemoveBufer(this._bufer.ViewId);
            WindowLevelContext.Current.RerenderBufers();

            tabIndex = this._GetNearestTabIndex(tabIndex);

            if (tabIndex > 0)
            {
                var keyboard = new KeyboardEmulator();

                if (tabIndex < this._clipboardBuferService.BufersCount - tabIndex)
                {
                    new KeyboardEmulator().PressTab((uint)tabIndex);
                }
                else
                {
                    new KeyboardEmulator().HoldDownShift().PressTab((uint)(this._clipboardBuferService.BufersCount - tabIndex));
                }
            }
        }

        private int _GetNearestTabIndex(int tabIndex)
        {
            if (tabIndex == this._clipboardBuferService.BufersCount)
            {
                tabIndex -= 1;
            }

            return tabIndex;
        }
    }
}