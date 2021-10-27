namespace BuferMAN.Infrastructure.Storage
{
    public interface ISessionManager
    {
        void SaveSession();
        bool IsLatestSessionSaved();
        void RestoreSession();
    }
}
