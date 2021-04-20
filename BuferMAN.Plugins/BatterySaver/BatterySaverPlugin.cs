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

        private bool _enabled;
        private readonly bool _batteryAvailable;

        private readonly Timer _timer = new Timer();

        public BatterySaverPlugin()
        {
            var status = SystemInformation.PowerStatus;
            this._batteryAvailable = status.BatteryChargeStatus != BatteryChargeStatus.Unknown &&
                status.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery;

            this.Enabled = true;
        }

        public override void Initialize(IBufermanHost bufermanHost)
        {
            base.Initialize(bufermanHost);

            if (this._batteryAvailable)
            {
                this._timer.Interval = this._settings.IntervalInSeconds * 1000;
                this._timer.Tick += this._BatteryCheckHandler;
            }
        }

        private BufermanMenuItem _CreateMainMenuItem()
        {
            if (this._batteryAvailable)
            {
                return this.BufermanHost.CreateMenuItem(this.Name, this.BatterySaverMenuItem_Click);
            }
            else
            {
                return null;
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

        private void BatterySaverMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = !this.Enabled;
            // Open settings window: show current battery state and history of changes (chart: green if online, red if offline)
        }

        public override BufermanMenuItem CreateMainMenuItem()
        {
            if (this._batteryAvailable)
            {
                return this._CreateMainMenuItem();
            }
            else
            {
                return null;
            }
        }

        public override bool Enabled
        {
            get
            {
                return this._enabled;
            }
            set
            {
                if (this._batteryAvailable)
                {
                    if (value)
                    {
                        this._timer.Start();
                    }
                    else
                    {
                        this._timer.Stop();
                    }

                    this._enabled = value;
                }
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
