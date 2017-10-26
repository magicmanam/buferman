using System;

namespace ClipboardViewer
{
    interface IBuferHandlersWrapper
    {
        void ChangeText(object sender, EventArgs e);
        void DeleteBufer(object sender, EventArgs e);
        void MarkAsPersistent(object sender, EventArgs e);
    }
}