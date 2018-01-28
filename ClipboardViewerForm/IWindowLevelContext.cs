using System;

namespace ClipboardViewerForm
{
    interface IWindowLevelContext
    {
        void HideWindow();
        void RerenderBufers();
        IntPtr WindowHandle { get; }
    }
}