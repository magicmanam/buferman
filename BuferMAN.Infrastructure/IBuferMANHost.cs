using BuferMAN.Infrastructure;
using System;

namespace BuferMAN.Infrastructure
{
    public interface IBuferMANHost
    {
        event EventHandler ClipbordUpdated;
        event EventHandler WindowActivated;
        INotificationEmitter NotificationEmitter { get; }
        bool IsVisible { get; }
        void SetStatusBarText(string newText);
        void OnFullBuferMAN(object sender, EventArgs e);
        void BuferFocused(object sender, BuferFocusedEventArgs e);
        void ActivateWindow();
        void HideWindow();
        void RerenderBufers();
        void Exit();
        void GenerateMenu();
    }
}
