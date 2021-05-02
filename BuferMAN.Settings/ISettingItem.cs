namespace BuferMAN.Settings
{
    public interface ISettingItem
    {
        void RestoreDefault();
        void Save();
        bool IsSavedValueDefault { get; }
        bool IsDirty { get; }
    }
}