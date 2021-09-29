using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Plugins;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Stats.BuferMAN.Plugin
{
    public class StatsPlugin : BufermanPluginBase
    {
        private BufermanMenuItem _mainMenuItem;
        private readonly DateTime _startTime = DateTime.Now;
        private DateTime _latestCopyDay = DateTime.Now.Date;

        public override string Name
        {
            get
            {
                return Resource.StatsPlugin;
            }
        }

        public StatsPlugin()
        {
            this.Available = true;
            this.Enabled = true;
        }

        public long CopiesCount { get; private set; } = 0;
        public long CurrentDayCopiesCount { get; set; } = 0;

        public override void Initialize(IBufermanHost bufermanHost)
        {
            base.Initialize(bufermanHost);

            if (this.Enabled)
            {
                this.BufermanHost.AddStatusLinePart(null, new Icon(SystemIcons.Information, 30, 30), this._StatisticsLabel_MouseEnter);

                bufermanHost.ClipbordUpdated += this._BufermanHost_ClipbordUpdated;
            }
        }

        private void _StatisticsLabel_MouseEnter(object sender, EventArgs e)
        {
            (sender as Action<string>)(this._GetStatisticsText());
        }

        private void _IncrementCopiesCounters()
        {
            this.CopiesCount++;

            var currentDate = DateTime.Now.Date;
            if (currentDate != this._latestCopyDay)
            {
                this._latestCopyDay = currentDate;
                this.CurrentDayCopiesCount = 0;
            }

            this.CurrentDayCopiesCount++;
        }

        private string _GetStatisticsText()
        {
            return this._startTime.Date == DateTime.Now.Date ?
                string.Format(Resource.TodayStatsInfo, this._startTime, this.CurrentDayCopiesCount) :
                string.Format(Resource.StatsInfo, this._startTime, this.CopiesCount, this.CurrentDayCopiesCount);
        }

        private void _BufermanHost_ClipbordUpdated(object sender, EventArgs e)
        {
            if (this.Enabled)
            {
                this._IncrementCopiesCounters();

                if (this.CopiesCount == 100)
                {
                    this.BufermanHost.NotificationEmitter.ShowInfoNotification(Resource.NotifyIcon100Congrats, 2500);
                }
                else if (this.CopiesCount == 1000)
                {
                    this.BufermanHost.NotificationEmitter.ShowInfoNotification(Resource.NotifyIcon1000Congrats, 2500);
                }
            }
        }

        public override BufermanMenuItem CreateMainMenuItem()
        {
            this._mainMenuItem = this.BufermanHost.CreateMenuItem(() => this.Name, this._StatsMenuItem_Click);
            this._mainMenuItem.Checked = this.Available && this.Enabled;

            return this._mainMenuItem;
        }

        private void _StatsMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = !this.Enabled;
            this._mainMenuItem.Checked = this.Enabled;
        }
    }
}
