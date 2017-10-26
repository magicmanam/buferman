using System;

namespace ClipboardViewer
{
    interface IWindowActivationHandler
    {
        void OnActivated(object sender, EventArgs e);
    }
}