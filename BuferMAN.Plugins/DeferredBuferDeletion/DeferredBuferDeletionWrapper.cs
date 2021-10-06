using magicmanam.Windows;
using BuferMAN.Infrastructure;
using System;
using System.Windows.Forms;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.ContextMenu;

namespace BuferMAN.Plugins.DeferredBuferDeletion
{
    internal class DeferredBuferDeletionWrapper
    {
        private readonly BuferContextMenuState _buferContextMenuState;
        private readonly IBufermanHost _bufermanHost;
        private Timer _timer = null;
        private bool _isTooltipTitleChanged = false;
        private readonly ITime _time;

        private BufermanMenuItem _separatorItem;
        private BufermanMenuItem _cancelDeletionMenuItem;

        public DeferredBuferDeletionWrapper(BuferContextMenuState buferContextMenuState, IBufermanHost bufermanHost, ITime time)
        {
            this._bufermanHost = bufermanHost;
            this._buferContextMenuState = buferContextMenuState;
            this._time = time;

            var deleteBuferNowMenuItem = bufermanHost.CreateMenuItem(() => Resource.DeleteBuferNowMenuItem, this._DeleteBuferImmediately);
            deleteBuferNowMenuItem.ShortCut = Shortcut.Del;
            buferContextMenuState.DeleteBuferMenuItem.AddMenuItem(deleteBuferNowMenuItem);
            buferContextMenuState.DeleteBuferMenuItem.AddMenuItem(bufermanHost.CreateMenuItem(() => string.Format(Resource.DeleteBuferInNMinutesMenuItem, 1), this._GetDeferredDeleteBuferHandler(1)));
            buferContextMenuState.DeleteBuferMenuItem.AddMenuItem(bufermanHost.CreateMenuItem(() => string.Format(Resource.DeleteBuferInNMinutesMenuItem, 10), this._GetDeferredDeleteBuferHandler(10)));
            buferContextMenuState.DeleteBuferMenuItem.AddMenuItem(bufermanHost.CreateMenuItem(() => string.Format(Resource.DeleteBuferInNMinutesMenuItem, 45), this._GetDeferredDeleteBuferHandler(45)));

            buferContextMenuState.PinnedBufer += (object sender, BuferPinnedEventArgs args) =>
            {
                if (this.IsDeferredDeletionActivated())
                {
                    this.CancelDeferredBuferDeletion(sender, args);
                }
            };
        }

        private void _DeleteBuferImmediately(object sender, EventArgs e)
        {
            if (this.IsDeferredDeletionActivated())
            {
                this.CancelDeferredBuferDeletion(sender, e);
            }

            if (this._buferContextMenuState.Bufer.TabIndex > 0)
            {
                new KeyboardEmulator().HoldDownShift().PressTab();
            }

            this._buferContextMenuState.RemoveBufer();
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

            if (this._isTooltipTitleChanged)
            {
                this._buferContextMenuState.Bufer.SetMouseOverToolTipTitle(null);
                this._isTooltipTitleChanged = false;
            }
        }

        private void _UncheckAllDeleteOptions()
        {
            foreach (var menuItem in this._buferContextMenuState.DeleteBuferMenuItem.Children)
            {
                menuItem.Checked = false;
            }
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
            this._buferContextMenuState.RemoveBufer();

            this.CancelDeferredBuferDeletion(sender, e);
        }

        private void _AddCancelDeletionMenuItem()
        {
            this._separatorItem = this._buferContextMenuState.DeleteBuferMenuItem.AddSeparator();
            var deletionTime = this._time.LocalTime.AddMilliseconds(this._timer.Interval).ToLocalTime();

            this._cancelDeletionMenuItem = this._bufermanHost
                .CreateMenuItem(() => string.Format(Resource.CancelDeferredDeletionMenuItem, deletionTime),
                                this.CancelDeferredBuferDeletion);

            this._buferContextMenuState.DeleteBuferMenuItem.AddMenuItem(this._cancelDeletionMenuItem);

            if (string.IsNullOrEmpty(this._buferContextMenuState.Bufer.MouseOverTooltip.ToolTipTitle))
            {
                this._buferContextMenuState.Bufer.SetMouseOverToolTipTitle(string.Format(Resource.DeferredBuferDeletionTime, deletionTime));
                this._isTooltipTitleChanged = true;
            }
        }
    }
}