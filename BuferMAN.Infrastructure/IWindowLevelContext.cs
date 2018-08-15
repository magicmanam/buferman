using System;

namespace BuferMAN.Infrastructure
{
    public interface IWindowLevelContext
    {
        void HideWindow();
        void ActivateWindow();
        void RerenderBufers();
        IntPtr WindowHandle { get; }
    }
}