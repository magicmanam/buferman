namespace BuferMAN.Infrastructure.Environment
{
    public interface IUserInteraction
    {
        bool? ShowYesNoCancelPopup(string text, string caption);
        void ShowPopup(string text, string caption);
    }
}
