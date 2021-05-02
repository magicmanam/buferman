﻿using System.Drawing;

namespace BuferMAN.Infrastructure.Settings
{
    public interface IProgramSettingsSetter
    {
        bool ShowUserModeNotification { set; }
        Color CurrentBuferBackgroundColor { set; }
        Color BuferDefaultBackgroundColor { set; }
        Color PinnedBuferBackgroundColor { set; }
        int FocusTooltipDuration { set; }
        bool ShowFocusTooltip { set; }
        bool IsDirty { get; }
        bool IsDefault { get; }
        void Save();
        void RestoreDefault();
    }
}
