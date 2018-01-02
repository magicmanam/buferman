using System;

namespace ClipboardViewerForm
{
    interface IBuferSelectionHandler
    {
        void DoOnClipSelection(object sender, EventArgs e);
    }
}