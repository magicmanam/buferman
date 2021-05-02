using BuferMAN.Infrastructure.Settings;
using BuferMAN.Models;
using System.Collections.Generic;
using System.Drawing;

namespace BuferMAN.Settings
{
    internal class ProgramSettings : IProgramSettingsGetter, IProgramSettingsSetter
    {
        //private Dictionary<string, ISettingItem> _settingItems = new Dictionary<string, ISettingItem>();
        private SettingItem<int> _buferDefaultBackgroundColorSetting;
        private SettingItem<bool> _showUserModeNotificationSetting;
        private SettingItem<bool> _showFocusTooltipSetting;
        private Color _pinnedBuferBackgroundColor;
        private Color _currentBuferBackgroundColor;
        private int _focusTooltipDuration;

        public ProgramSettings()
        {
            this._buferDefaultBackgroundColorSetting = new SettingItem<int>(Color.Silver.ToArgb(), User.Default.BuferDefaultBackgroundColor);
            this._showUserModeNotificationSetting = new SettingItem<bool>(true, User.Default.ShowUserModeNotification);
            this._pinnedBuferBackgroundColor = this.PinnedBuferBackgroundColor;
            this._currentBuferBackgroundColor = this.CurrentBuferBackgroundColor;
            this._focusTooltipDuration = this.FocusTooltipDuration;
            this._showFocusTooltipSetting = new SettingItem<bool>(true, User.Default.ShowFocusTooltip);
        }

        public IEnumerable<BufersStorageModel> StoragesToLoadOnStart =>
            new List<BufersStorageModel> {
                new BufersStorageModel
                {
                    StorageType = BufersStorageType.JsonFile,
                    StorageAddress = this.DefaultBufersFileName
                }
            };

        public string DefaultBufersFileName => "bufers.json";

        public int MaxBufersCount => 30;

        public int ExtraBufersCount => 25;

        public int MaxBuferPresentationLength => 2300;//Limits: low 2000, high 5000

        public Color BuferDefaultBackgroundColor
        {
            get
            {
                return Color.FromArgb(this._buferDefaultBackgroundColorSetting.SavedValue);
            }
            set
            {
                this._buferDefaultBackgroundColorSetting.CurrentValue = value.ToArgb();
            }
        }

        public Color CurrentBuferBackgroundColor
        {
            get
            {
                return Color.FromArgb(User.Default.CurrentBuferBackgroundColor);
            }
            set
            {
                this._currentBuferBackgroundColor = value;
            }
        }

        public Color FocusedBuferBackgroundColor
        {
            get
            {
                return Color.LightSteelBlue;
            }
        }

        public Color PinnedBuferBackgroundColor
        {
            get
            {
                return Color.FromArgb(User.Default.PinnedBuferBackgroundColor);// Color.LightSlateGray;
            }
            set
            {
                this._pinnedBuferBackgroundColor = value;
            }
        }

        public Color PinnedCurrentBuferBackColor
        {
            get
            {
                return this.PinnedBuferBackgroundColor;// Color.LawnGreen;
            }
        }

        public bool ShowUserModeNotification
        {
            get
            {
                return this._showUserModeNotificationSetting.SavedValue;
            }
            set
            {
                this._showUserModeNotificationSetting.CurrentValue = value;
            }
        }

        public int FocusTooltipDuration
        {
            get
            {
                return User.Default.FocusTooltipDuration;
            }
            set
            {
                this._focusTooltipDuration = value;
            }
        }

        public bool ShowFocusTooltip
        {
            get
            {
                return this._showFocusTooltipSetting.SavedValue;
            }
            set
            {
                this._showFocusTooltipSetting.CurrentValue = value;
            }
        }

        public void RestoreDefault()
        {
            this._showUserModeNotificationSetting.RestoreDefault();
            this._buferDefaultBackgroundColorSetting.RestoreDefault();
            this._showFocusTooltipSetting.RestoreDefault();
            this._pinnedBuferBackgroundColor = Color.LightSlateGray;
            this._currentBuferBackgroundColor = Color.FromArgb(User.Default.BuferDefaultBackgroundColor);//Or Color.LightGreen
            this._focusTooltipDuration = 2500;
        }

        public bool IsDefault
        {
            get
            {
                return User.Default.ShowFocusTooltip == true &&
                       User.Default.FocusTooltipDuration == 2500 &&
                       this._showUserModeNotificationSetting.IsSavedValueDefault &&
                       User.Default.PinnedBuferBackgroundColor == Color.LightSlateGray.ToArgb() &&
                       User.Default.CurrentBuferBackgroundColor == User.Default.BuferDefaultBackgroundColor &&
                       this._buferDefaultBackgroundColorSetting.IsSavedValueDefault;
            }
        }

        public void Save()
        {
            User.Default.BuferDefaultBackgroundColor = this._buferDefaultBackgroundColorSetting.Save().CurrentValue;
            User.Default.ShowUserModeNotification = this._showUserModeNotificationSetting.Save().CurrentValue;
            User.Default.PinnedBuferBackgroundColor = this._pinnedBuferBackgroundColor.ToArgb();
            User.Default.CurrentBuferBackgroundColor = this._currentBuferBackgroundColor.ToArgb();

            User.Default.FocusTooltipDuration = this._focusTooltipDuration;
            User.Default.ShowFocusTooltip = this._showFocusTooltipSetting.Save().CurrentValue;

            User.Default.Save();
        }

        public bool IsDirty
        {
            get
            {
                return this._showFocusTooltipSetting.IsDirty ||
                       User.Default.FocusTooltipDuration != this._focusTooltipDuration ||
                       this._showUserModeNotificationSetting.IsDirty ||
                       User.Default.PinnedBuferBackgroundColor != this._pinnedBuferBackgroundColor.ToArgb() ||
                       User.Default.CurrentBuferBackgroundColor != this._currentBuferBackgroundColor.ToArgb() ||
                       this._buferDefaultBackgroundColorSetting.IsDirty;
            }
        }
    }
}
