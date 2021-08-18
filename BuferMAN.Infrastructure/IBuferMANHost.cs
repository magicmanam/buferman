using BuferMAN.Infrastructure.Environment;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.View;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure
{
    public interface IBufermanHost
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
        void RerenderUserManual();
        void SetCurrentBufer(BuferViewModel bufer);
        void Exit();
        void Start(IBufermanApplication bufermanApp, bool isAdmin);
        void SetMainMenu(IEnumerable<BufermanMenuItem> menuItems);
        void SetTrayMenu(IEnumerable<BufermanMenuItem> menuItems);
        BufermanMenuItem CreateMenuItem(Func<string> textFn, EventHandler eventHandler = null);
        BufermanMenuItem CreateMenuItem(string text, EventHandler eventHandler = null);
        BufermanMenuItem CreateMenuSeparatorItem();
        IUserInteraction UserInteraction { get; }
        BuferViewModel LatestFocusedBufer { get; set; }
    }
}
