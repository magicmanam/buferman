using BuferMAN.Models;
using System.Collections.Generic;
using System.Drawing;

namespace BuferMAN.Infrastructure.Settings
{
    public interface IProgramSettings
    {
        IEnumerable<BufersStorageModel> StoragesToLoadOnStart { get; }
        string DefaultBufersFileName { get; }
        int MaxBufersCount { get; }
        int ExtraBufersCount { get; } // Can not be big, because rendering is too slow cause of auto keyboard emulation.
        int MaxBuferPresentationLength { get; }
        int BuferTooltipDuration { get; }
        Color BuferDefaultBackColor { get; }
        Color CurrentBuferBackColor { get; }
        Color FocusedBuferBackColor { get; }
        Color PinnedBuferBackColor { get; }
        Color PinnedCurrentBuferBackColor { get; }
        bool ShowUserModeNotification { get; }
    }
}
