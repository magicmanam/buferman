using BuferMAN.Models;
using System.Collections.Generic;
using System.Drawing;

namespace BuferMAN.Infrastructure.Settings
{
    public interface IProgramSettingsGetter
    {
        IEnumerable<BufersStorageModel> StoragesToLoadOnStart { get; }
        string SessionsRootDirectory { get; }
        string DefaultBufersFileName { get; }
        int MaxBufersCount { get; }
        int ExtraBufersCount { get; } // TODO (m) Replace with adding scrolling bufers. Can not be big, because rendering is too slow cause of keyboard emulation.
        int FocusTooltipDuration { get; }
        Color BuferDefaultBackgroundColor { get; }
        Color CurrentBuferBackgroundColor { get; }
        Color FocusedBuferBackgroundColor { get; }
        Color PinnedBuferBackgroundColor { get; }
        Color PinnedCurrentBuferBackColor { get; }
        bool ShowUserModeNotification { get; }
        bool RestorePreviousSession { get; }
        bool ShowFocusTooltip { get; }
        int EscHotKeyIntroductionCounter { get; }
        int ClosingWindowExplanationCounter { get; }
        int HttpUrlBuferExplanationCounter { get; }
        bool IsBuferClickingExplained { get; }
        int MaxBuferLengthToShowOnAliasCreation { get; }
        int MaxFilePathLengthForBuferTitle { get; }
    }
}
