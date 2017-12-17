using System;

namespace ClipboardViewer.Window
{
    interface IWindowActivationHandler
    {
        void OnActivated(object sender, EventArgs e);
    }
}