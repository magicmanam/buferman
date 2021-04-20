namespace BuferMAN.Plugins.BatterySaver
{
    class BatterySaverPluginSettings
    {
        private int _lowLimit = 25;
        private int _highLimit = 90;

        public int IntervalInSeconds { get; set; } = 60;

        public int HighLimitPercent
        {
            get
            {
                return _highLimit;
            }
            set
            {
                if (value < 100 || value > this.LowLimitPercent)
                {
                    _highLimit = value;
                }
            }
        }

        public int LowLimitPercent
        {
            get
            {
                return _lowLimit;
            }
            set
            {
                if (value > 9 && value < this.HighLimitPercent)
                {
                    _lowLimit = value;
                }
            }
        }
    }
}
