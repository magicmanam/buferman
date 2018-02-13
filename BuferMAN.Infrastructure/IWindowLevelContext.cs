using System;

namespace BuferMAN.Infrastructure
{
    public interface IWindowLevelContext
    {
        void HideWindow();
        void RerenderBufers();
        IntPtr WindowHandle { get; }
    }
}