using BuferMAN.Infrastructure;
using BuferMAN.Menu.Properties;
using System.Windows.Forms;

namespace BuferMAN.Menu.Plugins
{
    public class BatterySaverMenuItem : MenuItem
    {
        private readonly INotificationEmitter _notificationEmitter;
        private const string NOTIFICATION_TITLE = "Battery saver plugin";

        public BatterySaverMenuItem(INotificationEmitter notificationEmitter)
        {
            this._notificationEmitter = notificationEmitter;

            this.Text = Resource.MenuPluginsBattery;
            this.Click += BatterySaverMenuItem_Click;

            var trickTimer = new Timer();
            trickTimer.Interval = 100 * 1000;// into settings
            trickTimer.Tick += BatteryCheckHandler;
            trickTimer.Start();
        }

        private void BatteryCheckHandler(object sender, System.EventArgs e)
        {
            var status = SystemInformation.PowerStatus;

            if (status.PowerLineStatus == PowerLineStatus.Offline && status.BatteryLifePercent < 0.15)// Into settings
            {
                this._notificationEmitter.ShowWarningNotification($"Please charge your battery ({(status.BatteryLifePercent * 100)}%) in order to save your battery.", 2500, NOTIFICATION_TITLE);
            }

            if (status.PowerLineStatus == PowerLineStatus.Online && status.BatteryLifePercent > 0.85)// Into settings
            {
                this._notificationEmitter.ShowWarningNotification($"Please uncharge your battery ({(status.BatteryLifePercent * 100)}%) in order to save your battery", 2500, NOTIFICATION_TITLE);
            }
        }

        private void BatterySaverMenuItem_Click(object sender, System.EventArgs e)
        {
            // Enable / disable battery saver plugin and open settings window!
            // Show current battery state and history of changes (chart: green if online, red if offline)
        }
    }
}
