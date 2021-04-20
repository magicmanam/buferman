using System;
using BuferMAN.Infrastructure;
using System.Windows.Forms;

namespace BuferMAN.WinForms
{
    public class NotificationEmitter : INotificationEmitter
    {
        private readonly NotifyIcon _notifyIcon;
        private string _defaultTitle;

        public NotificationEmitter(NotifyIcon notifyIcon, string defaultTitle)
        {
            this._notifyIcon = notifyIcon;
            this._defaultTitle = defaultTitle;
        }

        public void ShowWarningNotification(string alertText, int delay, string title = null)
        {
            this._ShowNotification(alertText, ToolTipIcon.Warning, delay, title);
        }

        public void ShowInfoNotification(string infoText, int delay, string title = null)
        {
            this._ShowNotification(infoText, ToolTipIcon.Info, delay, title);
        }

        private void _ShowNotification(string notificationText, ToolTipIcon icon, int delay, string title = null)
        {
            this._notifyIcon.ShowBalloonTip(delay, title ?? this._defaultTitle, notificationText, icon);
            //this._notifyIcon.BalloonTipClicked += _notifyIcon_BalloonTipClicked;
        }

        private void _notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            this._notifyIcon.BalloonTipClicked -= _notifyIcon_BalloonTipClicked;
            // Open plugin settings window.
        }
    }
}
