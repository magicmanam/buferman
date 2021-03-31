using BuferMAN.Clipboard;
using BuferMAN.ContextMenu.Properties;
using BuferMAN.Infrastructure;
using System;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.View;
using BuferMAN.Infrastructure.Menu;

namespace BuferMAN.ContextMenu
{
    public class DeleteClipMenuItem
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly BuferViewModel _bufer;
        private readonly Button _button;
        private readonly BuferMANMenuItem _menuItem;
        private readonly IBuferMANHost _buferMANHost;
        private Timer _timer = null;

        private BuferMANMenuItem _separatorItem;
        private BuferMANMenuItem _cancelDeletionMenuItem;

        public DeleteClipMenuItem(BuferMANMenuItem menuItem, IClipboardBuferService clipboardBuferService, BuferViewModel bufer, Button button, IBuferMANHost buferMANHost)
        {
            this._buferMANHost = buferMANHost;
            this._menuItem = menuItem;
            this._clipboardBuferService = clipboardBuferService;
            this._bufer = bufer;
            this._button = button;

            var deleteBuferNowMenuItem = buferMANHost.CreateMenuItem(Resource.DeleteBuferNowMenuItem, this._DeleteBuferImmediately);
            deleteBuferNowMenuItem.ShortCut = Shortcut.Del;
            menuItem.AddMenuItem(deleteBuferNowMenuItem);
            menuItem.AddMenuItem(buferMANHost.CreateMenuItem(string.Format(Resource.DeleteBuferInNMinutesMenuItem, 1), this._GetDeferredDeleteBuferHandler(1)));
            menuItem.AddMenuItem(buferMANHost.CreateMenuItem(string.Format(Resource.DeleteBuferInNMinutesMenuItem, 10), this._GetDeferredDeleteBuferHandler(10)));
            menuItem.AddMenuItem(buferMANHost.CreateMenuItem(string.Format(Resource.DeleteBuferInNMinutesMenuItem, 45), this._GetDeferredDeleteBuferHandler(45)));
        }

        private void _DeleteBuferImmediately(object sender, EventArgs e)
        {
            if (this._timer != null)
            {
                this._CancelDeferredBuferDeletion(sender, e);
            }

            var tabIndex = this._button.TabIndex;

            this._RemoveBufer();

            this._FocusNextBufer(tabIndex);
        }

        private void _CancelDeferredBuferDeletion(object sender, EventArgs e)
        {
            this._timer.Stop();
            this._timer.Dispose();
            this._timer = null;

            this._separatorItem.Remove();
            this._cancelDeletionMenuItem.Remove();
        }

        private void _RemoveBufer()
        {
            this._clipboardBuferService.RemoveBufer(this._bufer.ViewId);
            WindowLevelContext.Current.RerenderBufers();
        }

        private void _FocusNextBufer(int tabIndex)
        {
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

        private EventHandler _GetDeferredDeleteBuferHandler(int deleteInMinutes)
        {
            return (object sender, EventArgs e) =>
            {
                if (this._timer != null)
                {
                    this._CancelDeferredBuferDeletion(sender, e);
                }

                this._timer = new Timer
                {
                    Interval = deleteInMinutes * 60 * 1000
                };
                this._timer.Tick += this._OnTimerTick;
                this._timer.Start();

                foreach(var menuItem in this._menuItem.Children)
                {
                    menuItem.Checked = false;
                }
                (sender as MenuItem).Checked = true;// TODO : sender should be BuferMANMenuItem

                this._AddCancelDeletionMenuItem();
            };
        }

        private void _OnTimerTick(object sender, EventArgs e)
        {
            this._RemoveBufer();

            this._CancelDeferredBuferDeletion(sender, e);
        }

        private void _AddCancelDeletionMenuItem()
        {
            this._separatorItem = this._menuItem.AddSeparator();
            this._cancelDeletionMenuItem = this._buferMANHost
                .CreateMenuItem(string.Format(Resource.CancelDeferredDeletionMenuItem, DateTime.Now.AddMilliseconds(this._timer.Interval).ToLocalTime()),
                                this._CancelDeferredBuferDeletion);

            this._menuItem.AddMenuItem(this._cancelDeletionMenuItem);
        }
    }
}