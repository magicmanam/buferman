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

        public BatterySaverPlugin() : base(Resource.BatterySaverPlugin)
        { }

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

            var trickTimer = new Timer();
            trickTimer.Interval = INTERVAL_IN_SECONDS * 1000;
            trickTimer.Tick += BatteryCheckHandler;
            trickTimer.Start();

            return menuItem;
        }

        private void BatteryCheckHandler(object sender, EventArgs e)
        {
            var status = SystemInformation.PowerStatus;

            if (status.PowerLineStatus == PowerLineStatus.Offline && status.BatteryLifePercent * 100 < LowLimit)
            {
                this.BufermanHost.NotificationEmitter.ShowWarningNotification($"Please charge your battery ({(status.BatteryLifePercent * 100)}%) in order to save your battery's health.", 2500, NOTIFICATION_TITLE);
                // TODO (s) text into resources!
            }

            if (status.PowerLineStatus == PowerLineStatus.Online && status.BatteryLifePercent * 100 > HighLimit)
            {
                this.BufermanHost.NotificationEmitter.ShowWarningNotification($"Please uncharge your battery ({(status.BatteryLifePercent * 100)}%) in order to save your battery's health", 2500, NOTIFICATION_TITLE);
            }
        }

        private void BatterySaverMenuItem_Click(object sender, EventArgs e)
        {
            // Enable / disable battery saver plugin and open settings window!
            // Show current battery state and history of changes (chart: green if online, red if offline)
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
    }
}
