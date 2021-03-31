using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure.Menu
{
    public abstract class BuferMANMenuItem
    {
        public abstract string Text { get; set; }
        public abstract Shortcut ShortCut { get; set; }
        public abstract bool Enabled { get; set; }
        public abstract bool Checked { get; set; }
        public abstract void SetOnClickHandler(EventHandler click);
        public abstract void RemoveOnClickHandler(EventHandler click);
        public abstract void SetOnPopupHandler(EventHandler popup);
        public abstract void RemoveOnPopupHandler(EventHandler popup);
        public abstract BuferMANMenuItem AddSeparator();
        public abstract void AddMenuItem(BuferMANMenuItem menuItem);
        public abstract void Remove();
        public abstract IEnumerable<BuferMANMenuItem> Children { get; }
    }
}
