using BuferMAN.Infrastructure.Settings;
using BuferMAN.Models;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace BuferMAN.Settings
{
    internal class ProgramSettings : IProgramSettingsGetter, IProgramSettingsSetter
    {
        private IList<ISettingItem> _settingItems;

        private SettingItem<bool> _showUserModeNotificationSetting;
        private SettingItem<bool> _showFocusTooltipSetting;
        private SettingItem<int> _pinnedBuferBackgroundColorSetting;
        private SettingItem<int> _buferDefaultBackgroundColorSetting;
        private SettingItem<int> _currentBuferBackgroundColorSetting;
        private SettingItem<int> _focusTooltipDurationSetting;

        public ProgramSettings()
        {
            this._buferDefaultBackgroundColorSetting = new SettingItem<int>(
                Color.Silver.ToArgb(),
                User.Default.BuferDefaultBackgroundColor,
                (value) => { User.Default.BuferDefaultBackgroundColor = value; });

            this._showUserModeNotificationSetting = new SettingItem<bool>(
                true,
                User.Default.ShowUserModeNotification,
                (value) => { User.Default.ShowUserModeNotification = value; });

            this._pinnedBuferBackgroundColorSetting = new SettingItem<int>(
                Color.LightSlateGray.ToArgb(),
                User.Default.PinnedBuferBackgroundColor,
                (value) => { User.Default.PinnedBuferBackgroundColor = value; });

            this._currentBuferBackgroundColorSetting = new SettingItem<int>(
                Color.Silver.ToArgb(), // TODO (s) Consider new color: Color.LightGreen
                User.Default.CurrentBuferBackgroundColor,
                (value) => { User.Default.CurrentBuferBackgroundColor = value; });

            this._focusTooltipDurationSetting = new SettingItem<int>(
                2500,
                User.Default.FocusTooltipDuration,
                (value) => { User.Default.FocusTooltipDuration = value; });

            this._showFocusTooltipSetting = new SettingItem<bool>(
                true,
                User.Default.ShowFocusTooltip,
                (value) => { User.Default.ShowFocusTooltip = value; });

            this._settingItems = new List<ISettingItem>
            {
                this._buferDefaultBackgroundColorSetting,
                this._showUserModeNotificationSetting,
                this._showFocusTooltipSetting,
                this._focusTooltipDurationSetting,
                this._pinnedBuferBackgroundColorSetting,
                this._currentBuferBackgroundColorSetting
            };
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
                return Color.FromArgb(this._currentBuferBackgroundColorSetting.SavedValue);
            }
            set
            {
                this._currentBuferBackgroundColorSetting.CurrentValue = value.ToArgb();
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
                return Color.FromArgb(this._pinnedBuferBackgroundColorSetting.SavedValue);
            }
            set
            {
                this._pinnedBuferBackgroundColorSetting.CurrentValue = value.ToArgb();
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
                return this._focusTooltipDurationSetting.SavedValue;
            }
            set
            {
                this._focusTooltipDurationSetting.CurrentValue = value;
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
            foreach (var setting in this._settingItems)
            {
                setting.RestoreDefault();
            }
        }

        public bool IsDefault
        {
            get
            {
                return this._settingItems.All(s => s.IsSavedValueDefault);
            }
        }

        public void Save()
        {
            foreach (var setting in this._settingItems)
            {
                setting.Save();
            }

            User.Default.Save();
        }

        public bool IsDirty
        {
            get
            {
                return this._settingItems.Any(s => s.IsDirty);
            }
        }

        public string SessionsRootDirectory
        {
            get
            {
                return Directory.GetCurrentDirectory();
            }
        }
    }
}
