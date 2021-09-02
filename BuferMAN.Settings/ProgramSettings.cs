using BuferMAN.Infrastructure.Files;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace BuferMAN.Settings
{
    internal class ProgramSettings : IProgramSettingsGetter, IProgramSettingsSetter
    {
        private readonly IList<ISettingItem> _settingItems;

        private readonly SettingItem<bool> _showUserModeNotificationSetting;
        private readonly SettingItem<bool> _restorePreviousSession;
        private readonly SettingItem<bool> _showFocusTooltipSetting;
        private readonly SettingItem<int> _pinnedBuferBackgroundColorSetting;
        private readonly SettingItem<int> _buferDefaultBackgroundColorSetting;
        private readonly SettingItem<int> _currentBuferBackgroundColorSetting;
        private readonly SettingItem<int> _focusTooltipDurationSetting;

        public ProgramSettings(IFileStorage fileStorage)
        {
            this._buferDefaultBackgroundColorSetting = new SettingItem<int>(
                Color.Silver.ToArgb(),
                User.Default.BuferDefaultBackgroundColor,
                (value) => { User.Default.BuferDefaultBackgroundColor = value; });

            this._showUserModeNotificationSetting = new SettingItem<bool>(
                false,
                User.Default.ShowUserModeNotification,
                (value) => { User.Default.ShowUserModeNotification = value; });

            this._restorePreviousSession = new SettingItem<bool>(
                true,
                User.Default.RestorePreviousSession,
                (value) => { User.Default.RestorePreviousSession = value; });

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
                this._restorePreviousSession,
                this._showFocusTooltipSetting,
                this._focusTooltipDurationSetting,
                this._pinnedBuferBackgroundColorSetting,
                this._currentBuferBackgroundColorSetting
            };

            if (!fileStorage.FileExists(this.DefaultBufersFileName))
            {
                fileStorage.CreateFile(this.DefaultBufersFileName);
            }
        }

        public IEnumerable<BufersStorageModel> StoragesToLoadOnStart =>
            new List<BufersStorageModel> {
                new BufersStorageModel
                {
                    StorageType = BufersStorageType.JsonFile,
                    StorageAddress = this.DefaultBufersFileName
                }
            };

        public string DefaultBufersFileName => Path.Combine(this._bufermanDataFolder, "bufers.json");

        public int MaxBufersCount => 30;

        public int ExtraBufersCount => 25;

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

        public bool RestorePreviousSession
        {
            get
            {
                return this._restorePreviousSession.SavedValue;
            }
            set
            {
                this._restorePreviousSession.CurrentValue = value;
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

        public int EscHotKeyIntroductionCounter
        {
            get
            {
                return User.Default.EscHotKeyIntroductionCounter;
            }
            set
            {
                User.Default.EscHotKeyIntroductionCounter = value;
                User.Default.Save();
            }
        }

        public int ClosingWindowExplanationCounter
        {
            get
            {
                return User.Default.ClosingWindowExplanationCounter;
            }
            set
            {
                User.Default.ClosingWindowExplanationCounter = value;
                User.Default.Save();
            }
        }

        public int HttpUrlBuferExplanationCounter
        {
            get
            {
                return User.Default.HttpUrlBuferExplanationCounter;
            }
            set
            {
                User.Default.HttpUrlBuferExplanationCounter = value;
                User.Default.Save();
            }
        }

        public bool IsBuferClickingExplained
        {
            get
            {
                return User.Default.IsBuferClickingExplained;
            }
        }

        public void MarkThatBuferClickingWasExplained()
        {
            User.Default.IsBuferClickingExplained = true;
            User.Default.Save();
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

        public int MaxBuferLengthToShowOnAliasCreation { get; } = 100;

        public string SessionsRootDirectory
        {
            get
            {
                return Path.Combine(
                    this._bufermanDataFolder,
                    "session");// TODO (s) in constants
            }
        }

        private readonly string _bufermanDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BuferMAN");
    }
}
