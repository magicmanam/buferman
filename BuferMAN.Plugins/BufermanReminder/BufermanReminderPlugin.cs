using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using System;

namespace BuferMAN.Plugins.BufermanReminder
{
    public class BufermanReminderPlugin : BufermanPluginBase
    {
        private BufermanMenuItem _mainMenuItem;
        private DateTime _lastInactivityTime;
        private readonly ITime _time;

        public override string Name
        {
            get
            {
                return Resource.BufermanReminderPlugin;
            }
        }

        public BufermanReminderPlugin(ITime time)
        {
            this._time = time;
            this.Available = true;
            this.Enabled = true;

            this._lastInactivityTime = time.LocalTime;
        }

        public override void Initialize(IBufermanHost bufermanHost)
        {
            base.Initialize(bufermanHost);

            this._lastInactivityTime = this._time.LocalTime;

            bufermanHost.WindowHidden += BufermanHost_WindowHidden;
            bufermanHost.WindowActivated += BufermanHost_WindowActivated;
            bufermanHost.ClipbordUpdated += BufermanHost_ClipbordUpdated;
        }

        private void BufermanHost_ClipbordUpdated(object sender, EventArgs e)
        {
            if (this._time.LocalTime > this._lastInactivityTime.AddDays(1))
            {
                this.BufermanHost.NotificationEmitter.ShowInfoNotification(Resource.ReminderPluginText, 3000);
            }
        }

        private void BufermanHost_WindowActivated(object sender, EventArgs e)
        {
            this._lastInactivityTime = new DateTime(3000, 12, 31);
        }

        private void BufermanHost_WindowHidden(object sender, EventArgs e)
        {
            this._lastInactivityTime = this._time.LocalTime;
        }

        public override BufermanMenuItem CreateMainMenuItem()
        {
            this._mainMenuItem = this.BufermanHost.CreateMenuItem(() => this.Name, this._ReminderBufermanMenuItem_Click);
            this._mainMenuItem.Checked = this.Available && this.Enabled;

            return this._mainMenuItem;
        }

        private void _ReminderBufermanMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = !this.Enabled;
            this._mainMenuItem.Checked = this.Enabled;
        }
    }
}
