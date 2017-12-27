using System;

namespace ClipboardViewerForm.Window
{
    interface IWindowActivationHandler
    {
        void OnActivated(object sender, EventArgs e);
    }
}