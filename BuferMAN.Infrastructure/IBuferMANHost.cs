using BuferMAN.Infrastructure.Menu;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

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
        void SetOnKeyDown(KeyEventHandler keyDown);
        void ActivateWindow();
        void HideWindow();
        void RerenderBufers();
        void Exit();
        void Start();
        void SetMainMenu(IEnumerable<BuferMANMenuItem> menuItems);
        BuferMANMenuItem CreateMenuItem(string text, EventHandler eventHandler = null);
        BuferMANMenuItem CreateMenuSeparatorItem();
        bool? ShowYesNoCancelPopup(string text, string caption);
        void ShowPopup(string text, string caption);
    }
}
