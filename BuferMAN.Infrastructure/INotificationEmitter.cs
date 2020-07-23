namespace BuferMAN.Infrastructure
{
    public interface INotificationEmitter
    {
        void ShowInfoNotification(string infoText, int delay, string title = null);
        void ShowWarningNotification(string alertText, int delay, string title = null);
    }
}
