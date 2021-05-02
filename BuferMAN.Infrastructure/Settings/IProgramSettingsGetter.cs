﻿using BuferMAN.Models;
using System.Collections.Generic;
using System.Drawing;

namespace BuferMAN.Infrastructure.Settings
{
    public interface IProgramSettingsGetter
    {
        IEnumerable<BufersStorageModel> StoragesToLoadOnStart { get; }
        string DefaultBufersFileName { get; }
        int MaxBufersCount { get; }
        int ExtraBufersCount { get; } // Can not be big, because rendering is too slow cause of auto keyboard emulation.
        int MaxBuferPresentationLength { get; }
        int FocusTooltipDuration { get; }
        Color BuferDefaultBackgroundColor { get; }
        Color CurrentBuferBackgroundColor { get; }
        Color FocusedBuferBackgroundColor { get; }
        Color PinnedBuferBackgroundColor { get; }
        Color PinnedCurrentBuferBackColor { get; }
        bool ShowUserModeNotification { get; }
        bool ShowFocusTooltip { get; }
    }
}