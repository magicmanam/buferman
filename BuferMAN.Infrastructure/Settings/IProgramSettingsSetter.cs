using System.Drawing;

namespace BuferMAN.Infrastructure.Settings
{
    public interface IProgramSettingsSetter
    {
        bool ShowUserModeNotification { set; }
        bool RestorePreviousSession { set; }
        Color CurrentBuferBackgroundColor { set; }
        Color BuferDefaultBackgroundColor { set; }
        Color PinnedBuferBackgroundColor { set; }
        int FocusTooltipDuration { set; }
        bool ShowFocusTooltip { set; }
        int EscHotKeyIntroductionCounter { set; }
        int ClosingWindowExplanationCounter { set; }
        int HttpUrlBuferExplanationCounter { set; }
        string PreferredTwoLetterISOLanguageName { set; }
        void MarkThatBuferClickingWasExplained();
        bool IsDirty { get; }
        bool IsDefault { get; }
        void Save();
        void RestoreDefault();
    }
}
