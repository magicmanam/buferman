using BuferMAN.Infrastructure.Environment;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.View;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure
{
    public interface IBufermanHost
    {
        event EventHandler ClipbordUpdated;
        event EventHandler WindowActivated;
        event EventHandler WindowHidden;
        INotificationEmitter NotificationEmitter { get; }
        bool IsVisible { get; }
        void SetStatusBarText(string newText);
        void OnFullBuferMAN(object sender, EventArgs e);
        void BuferFocused(object sender, BuferFocusedEventArgs e);
        void SetOnKeyDown(KeyEventHandler keyDown);
        void ActivateWindow();
        void HideWindow();
        void RerenderBufers(IEnumerable<BuferViewModel> temporaryBuferViewModels, IEnumerable<BuferViewModel> pinnedBuferViewModels);
        void RerenderUserManual();
        Guid CurrentBuferViewId { get; set; }
        void Exit();
        void Start(IBufermanApplication bufermanApp, bool isAdmin);
        void SetMainMenu(IEnumerable<BufermanMenuItem> menuItems);
        void SetTrayMenu(IEnumerable<BufermanMenuItem> menuItems);
        void AddStatusLinePart(string text, Icon icon, EventHandler mouseEnterHandler);
        void RefreshUI(IEnumerable<BuferViewModel> temporaryBuferViewModels, IEnumerable<BuferViewModel> pinnedBuferViewModels);
        BufermanMenuItem CreateMenuItem(Func<string> textFn, EventHandler eventHandler = null);
        BufermanMenuItem CreateMenuItem(string text, EventHandler eventHandler = null);
        BufermanMenuItem CreateMenuSeparatorItem();
        IUserInteraction UserInteraction { get; }
        BuferViewModel LatestFocusedBufer { get; set; }
        int InnerAreaWidth { get; }
        IEnumerable<IBufer> Bufers { get; }
        void AddBufer(IBufer bufer);
        void RemoveBufer(IBufer bufer);
        void SetPinnedBufersDividerY(int y);
        int PinnedBufersDividerHeight { get; }
        void SuspendLayoutLogic();
        void ResumeLayoutLogic();
        void ChangeLanguage(string shortLanguage);
        event EventHandler UILanguageChanged;
    }
}
