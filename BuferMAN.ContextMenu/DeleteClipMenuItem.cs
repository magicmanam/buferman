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
        private Timer _timer = new Timer();
        private bool _deleted;

        public DeleteClipMenuItem(IClipboardBuferService clipboardBuferService, BuferViewModel bufer, Button button)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._bufer = bufer;
            this._button = button;
            this.Text = Resource.DeleteClipMenuItem;

            this.MenuItems.Add(new MenuItem(Resource.DeleteBuferNowMenuItem, this._DeleteBufer, Shortcut.Del));
            this.MenuItems.Add(new MenuItem(Resource.DeleteBuferIn10MinutesMenuItem, this._GetDeleteBuferHandler(10)));
            this.MenuItems.Add(new MenuItem(Resource.DeleteBuferIn30MinutesMenuItem, this._GetDeleteBuferHandler(30)));
        }

        private EventHandler _GetDeleteBuferHandler(int deleteInMinutes)
        {
            return (object sender, EventArgs e) =>
            {
                this._deleted = false;// TODO : bad code. Remove after context menu regeneration on any button creation (recreation)
                _timer.Interval = deleteInMinutes * 60 * 1000;
                _timer.Tick += this._RemoveBufer;
                _timer.Start();
            };
        }

        private void _RemoveBufer(object sender, EventArgs e)
        {
            if (this._timer.Enabled)
            {
                this._timer.Stop();
            }

            if (!this._deleted)
            {
                this._clipboardBuferService.RemoveBufer(this._bufer.ViewId);
                WindowLevelContext.Current.RerenderBufers();

                this._deleted = true;
                this._timer.Dispose();
            }
        }

        private void _DeleteBufer(object sender, EventArgs e)
        {
            this._deleted = false;// TODO : bad code. Remove after context menu regeneration on any button creation (recreation)
            var tabIndex = this._button.TabIndex;
            this._RemoveBufer(sender, e);

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