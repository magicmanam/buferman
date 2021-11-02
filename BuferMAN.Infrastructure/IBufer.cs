using BuferMAN.Infrastructure.Menu;
using BuferMAN.View;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure
{
    public interface IBufer
    {
        IEnumerable<BufermanMenuItem> ContextMenu { get; set; }
        void SetText(string text);
        BuferViewModel ViewModel { get; set; }
        Font ApplyFontStyle(FontStyle style);
        void AddOnClickHandler(EventHandler onClick);
        IEnumerable<EventHandler> GetOnClickHandlers();
        void RemoveOnClickHandler(EventHandler onClick);
        void AddOnFocusHandler(EventHandler onFocus);
        IEnumerable<EventHandler> GetOnFocusHandlers();
        void RemoveOnFocusHandler(EventHandler onFocus);
        void AddOnUnfocusHandler(EventHandler onUnfocus);
        IEnumerable<EventHandler> GetOnUnfocusHandlers();
        void RemoveOnUnfocusHandler(EventHandler onUnfocus);
        Color BackColor { get; set; }
        ToolTip FocusTooltip { get; }
        ToolTip MouseOverTooltip { get; }
        int Width { get; set; }
        void SetMouseOverToolTip(string tooltipText);
        void SetMouseOverToolTipTitle(string tooltipTitle);
        void ShowFocusTooltip(string tooltipText, int durationInMilliseconds);
        void HideFocusTooltip();
        int TabIndex { get; set; }
        Point Location { get; set; }
        Font Font { get; }
        Image Image { get; set; }
        ContentAlignment ImageAlign { get; set; }
        ContentAlignment TextAlign { get; set; }
    }
}
