﻿using BuferMAN.Clipboard;
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
        private readonly BufermanMenuItem _menuItem;
        private readonly IBufermanHost _bufermanHost;
        private Timer _timer = null;

        private BufermanMenuItem _separatorItem;
        private BufermanMenuItem _cancelDeletionMenuItem;

        public DeleteClipMenuItem(BufermanMenuItem menuItem, IClipboardBuferService clipboardBuferService, BuferViewModel bufer, Button button, IBufermanHost bufermanHost)
        {
            this._bufermanHost = bufermanHost;
            this._menuItem = menuItem;
            this._clipboardBuferService = clipboardBuferService;
            this._bufer = bufer;
            this._button = button;

            var deleteBuferNowMenuItem = bufermanHost.CreateMenuItem(Resource.DeleteBuferNowMenuItem, this._DeleteBuferImmediately);
            deleteBuferNowMenuItem.ShortCut = Shortcut.Del;
            menuItem.AddMenuItem(deleteBuferNowMenuItem);
            menuItem.AddMenuItem(bufermanHost.CreateMenuItem(string.Format(Resource.DeleteBuferInNMinutesMenuItem, 1), this._GetDeferredDeleteBuferHandler(1)));
            menuItem.AddMenuItem(bufermanHost.CreateMenuItem(string.Format(Resource.DeleteBuferInNMinutesMenuItem, 10), this._GetDeferredDeleteBuferHandler(10)));
            menuItem.AddMenuItem(bufermanHost.CreateMenuItem(string.Format(Resource.DeleteBuferInNMinutesMenuItem, 45), this._GetDeferredDeleteBuferHandler(45)));
        }

        private void _DeleteBuferImmediately(object sender, EventArgs e)
        {
            if (this.IsDeferredDeletionActivated())
            {
                this.CancelDeferredBuferDeletion(sender, e);
            }

            var tabIndex = this._button.TabIndex;

            this._RemoveBufer();

            this._FocusNextBufer(tabIndex);
        }

        public bool IsDeferredDeletionActivated()
        {
            return this._timer != null;
        }

        public void CancelDeferredBuferDeletion(object sender, EventArgs e)
        {
            this._timer.Stop();
            this._timer.Dispose();
            this._timer = null;

            this._separatorItem.Remove();
            this._cancelDeletionMenuItem.Remove();
            this._UncheckAllDeleteOptions();
        }

        private void _UncheckAllDeleteOptions()
        {
            foreach (var menuItem in this._menuItem.Children)
            {
                menuItem.Checked = false;
            }
        }

        private void _RemoveBufer()
        {
            this._clipboardBuferService.RemoveBufer(this._bufer.ViewId);
            this._bufermanHost.RerenderBufers();
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
                if (this.IsDeferredDeletionActivated())
                {
                    this.CancelDeferredBuferDeletion(sender, e);
                }

                this._timer = new Timer
                {
                    Interval = deleteInMinutes * 60 * 1000
                };
                this._timer.Tick += this._OnTimerTick;
                this._timer.Start();

                this._UncheckAllDeleteOptions();
                (sender as MenuItem).Checked = true;// TODO (m) : sender should be BuferMANMenuItem, not MenuItem

                this._AddCancelDeletionMenuItem();
            };
        }

        private void _OnTimerTick(object sender, EventArgs e)
        {
            this._RemoveBufer();

            this.CancelDeferredBuferDeletion(sender, e);
        }

        private void _AddCancelDeletionMenuItem()
        {
            this._separatorItem = this._menuItem.AddSeparator();
            this._cancelDeletionMenuItem = this._bufermanHost
                .CreateMenuItem(string.Format(Resource.CancelDeferredDeletionMenuItem, DateTime.Now.AddMilliseconds(this._timer.Interval).ToLocalTime()),
                                this.CancelDeferredBuferDeletion);

            this._menuItem.AddMenuItem(this._cancelDeletionMenuItem);
        }
    }
}