using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using System;
using System.Windows.Forms;

namespace BuferMAN.Plugins.BatterySaver
{
    public class BatterySaverPlugin : BufermanPluginBase
    {
        private const string NOTIFICATION_TITLE = "Battery saver plugin";
        private readonly BatterySaverPluginSettings _settings = new BatterySaverPluginSettings();

        private readonly Timer _timer = new Timer();

        public BatterySaverPlugin()
        {
            var status = SystemInformation.PowerStatus;
            this.Available = status.BatteryChargeStatus != BatteryChargeStatus.Unknown &&
                status.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery;

            if (this.Available)
            {
                this.Enabled = true;
            }
        }

        public override void Initialize(IBufermanHost bufermanHost)
        {
            if (this.Available)
            {
                base.Initialize(bufermanHost);

                this._timer.Interval = this._settings.IntervalInSeconds * 1000;
                this._timer.Tick += this._BatteryCheckHandler;
            }
        }

        private void _BatteryCheckHandler(object sender, EventArgs e)
        {
            var status = SystemInformation.PowerStatus;

            if (status.PowerLineStatus == PowerLineStatus.Offline && status.BatteryLifePercent * 100 < this._settings.LowLimitPercent)
            {
                this.BufermanHost.NotificationEmitter.ShowWarningNotification(string.Format(Resource.BatterySaverPluginChargeNoteFormat, status.BatteryLifePercent * 100), 2500, NOTIFICATION_TITLE);
            }

            if (status.PowerLineStatus == PowerLineStatus.Online && status.BatteryLifePercent * 100 > this._settings.HighLimitPercent)
            {
                this.BufermanHost.NotificationEmitter.ShowWarningNotification(string.Format(Resource.BatterySaverPluginUnchargeNoteFormat, status.BatteryLifePercent * 100), 2500, NOTIFICATION_TITLE);
            }
        }

        private void _BatterySaverMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = !this.Enabled;
            // Open settings window: show current battery state and history of changes (chart: green if online, red if offline)
        }

        public override BufermanMenuItem CreateMainMenuItem()
        {
            return this.Available ?
                   this.BufermanHost.CreateMenuItem(this.Name, this._BatterySaverMenuItem_Click) :
                   throw new InvalidOperationException();
        }

        protected override void OnEnableChanged()
        {
            if (this.Enabled)
            {
                this._timer.Start();
            }
            else
            {
                this._timer.Stop();
            }
        }

        public override string Name
        {
            get
            {
                return Resource.BatterySaverPlugin;
            }
        }
    }
}
