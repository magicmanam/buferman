using BuferMAN.Infrastructure.Menu;
using System;
using System.Windows.Forms;

namespace BuferMAN.Plugins
{
    public class BatterySaverPlugin : BufermanPluginBase
    {
        private const string NOTIFICATION_TITLE = "Battery saver plugin";
        private const int INTERVAL_IN_SECONDS = 60;

        private int _highLimit = 90;
        private int _lowLimit = 25;
        private bool _enabled;

        private readonly Timer _timer = new Timer();

        public BatterySaverPlugin() : base(Resource.BatterySaverPlugin)
        {
            this._timer.Interval = INTERVAL_IN_SECONDS * 1000;
            this._timer.Tick += this._BatteryCheckHandler;

            this._enabled = true;
            this._timer.Start();
        }

        public int HighLimit
        {
            get
            {
                return _highLimit;
            }
            set
            {
                if (value < 100 || value > LowLimit)
                {
                    _highLimit = value;
                }
            }
        }

        public int LowLimit
        {
            get
            {
                return _lowLimit;
            }
            set
            {
                if (value > 9 && value < HighLimit)
                {
                    _lowLimit = value;
                }
            }
        }

        private BufermanMenuItem _CreateMainMenuItem()
        {
            var menuItem = this.BufermanHost.CreateMenuItem(this.Name, this.BatterySaverMenuItem_Click);
            
            return menuItem;
        }

        private void _BatteryCheckHandler(object sender, EventArgs e)
        {
            var status = SystemInformation.PowerStatus;

            if (status.PowerLineStatus == PowerLineStatus.Offline && status.BatteryLifePercent * 100 < LowLimit)
            {
                this.BufermanHost.NotificationEmitter.ShowWarningNotification(string.Format(Resource.BatterySaverPluginChargeNoteFormat, status.BatteryLifePercent * 100), 2500, NOTIFICATION_TITLE);
            }

            if (status.PowerLineStatus == PowerLineStatus.Online && status.BatteryLifePercent * 100 > HighLimit)
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
            var status = SystemInformation.PowerStatus;
            if (status.BatteryChargeStatus != BatteryChargeStatus.Unknown &&
                status.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery)
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
}
