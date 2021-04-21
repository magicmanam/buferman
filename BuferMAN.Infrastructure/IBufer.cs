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
        void SetContextMenu(IEnumerable<BufermanMenuItem> menuItems);
        string Text { get; set; }
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
        void SetToolTip(string text);
        Color BackColor { get; set; }
        ToolTip FocusTooltip { get; }// TODO (s) check that it needs more
        ToolTip MouseOverTooltip { get; }// TODO (s) check that it needs more
        int Width { get; set; }
        void SetMouseOverToolTip(string tooltipText);
        void ShowFocusTooltip(string tooltipText, int durationInMilliseconds);
        void HideFocusTooltip();
        int TabIndex { get; }
    }
}
