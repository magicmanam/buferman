using BuferMAN.Infrastructure.Settings;
using BuferMAN.Models;
using System.Collections.Generic;
using System.Drawing;

namespace BuferMAN.Settings
{
    internal class ProgramSettings : IProgramSettingsGetter, IProgramSettingsSetter
    {
        private Color _buferDefaultBackgroundColor;
        private bool _showUserModeNotification;
        private Color _pinnedBuferBackgroundColor;
        private Color _currentBuferBackgroundColor;
        private int _focusTooltipDuration;
        private bool _showFocusTooltip;

        public ProgramSettings()
        {
            this._buferDefaultBackgroundColor = this.BuferDefaultBackgroundColor;
            this._showUserModeNotification = this.ShowUserModeNotification;
            this._pinnedBuferBackgroundColor = this.PinnedBuferBackgroundColor;
            this._currentBuferBackgroundColor = this.CurrentBuferBackgroundColor;
            this._focusTooltipDuration = this.FocusTooltipDuration;
            this._showFocusTooltip = this.ShowFocusTooltip;
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
                return Color.FromArgb(User.Default.BuferDefaultBackgroundColor);
            }
            set
            {
                this._buferDefaultBackgroundColor = value;
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
                return User.Default.ShowUserModeNotification;
            }
            set
            {
                this._showUserModeNotification = value;
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
                return User.Default.ShowFocusTooltip;
            }
            set
            {
                this._showFocusTooltip = value;
            }
        }

        public void RestoreDefault()
        {
            this._showUserModeNotification = true;
            this._buferDefaultBackgroundColor = Color.Silver;
            this._pinnedBuferBackgroundColor = Color.LightSlateGray;
            this._currentBuferBackgroundColor = Color.FromArgb(User.Default.BuferDefaultBackgroundColor);//Or Color.LightGreen
            this._focusTooltipDuration = 2500;
            this._showFocusTooltip = true;
        }

        public bool IsDefault
        {
            get
            {
                return User.Default.ShowFocusTooltip != true ||
                       User.Default.FocusTooltipDuration != 2500 ||
                       User.Default.ShowUserModeNotification != true ||
                       User.Default.PinnedBuferBackgroundColor != Color.LightSlateGray.ToArgb() ||
                       User.Default.CurrentBuferBackgroundColor != User.Default.BuferDefaultBackgroundColor ||
                       User.Default.BuferDefaultBackgroundColor != Color.Silver.ToArgb();
            }
        }

        public void Save()
        {
            User.Default.BuferDefaultBackgroundColor = this._buferDefaultBackgroundColor.ToArgb();
            User.Default.ShowUserModeNotification = this._showUserModeNotification;
            User.Default.PinnedBuferBackgroundColor = this._pinnedBuferBackgroundColor.ToArgb();
            User.Default.CurrentBuferBackgroundColor = this._currentBuferBackgroundColor.ToArgb();

            User.Default.FocusTooltipDuration = this._focusTooltipDuration;
            User.Default.ShowFocusTooltip = this._showFocusTooltip;

            User.Default.Save();
        }

        public bool IsDirty
        {
            get
            {
                return User.Default.ShowFocusTooltip != this._showFocusTooltip ||
                       User.Default.FocusTooltipDuration != this._focusTooltipDuration ||
                       User.Default.ShowUserModeNotification != this._showUserModeNotification ||
                       User.Default.PinnedBuferBackgroundColor != this._pinnedBuferBackgroundColor.ToArgb() ||
                       User.Default.CurrentBuferBackgroundColor != this._currentBuferBackgroundColor.ToArgb() ||
                       User.Default.BuferDefaultBackgroundColor != this._buferDefaultBackgroundColor.ToArgb();
            }
        }
    }
}
