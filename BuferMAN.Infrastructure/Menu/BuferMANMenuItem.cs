using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure.Menu
{
    public abstract class BufermanMenuItem
    {
        public abstract string Text { get; set; }
        public abstract void TextRefresh();
        public abstract Shortcut ShortCut { get; set; }
        public abstract bool Enabled { get; set; }
        public abstract bool Checked { get; set; }
        public abstract void AddOnClickHandler(EventHandler onClick);
        public abstract IEnumerable<EventHandler> GetOnClickHandlers();
        public abstract void RemoveOnClickHandler(EventHandler onClick);
        public abstract void SetOnPopupHandler(EventHandler popup);
        public abstract void RemoveOnPopupHandler(EventHandler popup);
        public abstract BufermanMenuItem AddSeparator();
        public abstract void AddMenuItem(BufermanMenuItem menuItem);
        public abstract void Remove();
        public abstract IEnumerable<BufermanMenuItem> Children { get; }
    }
}
