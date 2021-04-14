namespace BuferMAN.Infrastructure.Environment
{
    public interface IUserInteraction
    {
        bool? ShowYesNoCancelPopup(string text, string caption);
        void ShowPopup(string text, string caption);
        string PromptPopup(string text, string title, string defaultValue);
    }
}
