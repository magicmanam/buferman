using System;

namespace BuferMAN.Infrastructure
{
    public interface IBuferSelectionHandler
    {
        void DoOnClipSelection(object sender, EventArgs e);
    }
}