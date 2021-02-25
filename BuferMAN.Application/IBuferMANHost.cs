using BuferMAN.Infrastructure;
using System;

namespace BuferMAN.Application
{
    public interface IBuferMANHost
    {
        event EventHandler ClipbordUpdated;
        event EventHandler WindowActivated;
        INotificationEmitter NotificationEmitter { get; }
        bool IsVisible { get; }
        void SetStatusBarText(string newText);
        void OnFullBuferMAN(object sender, EventArgs e);
    }
}
