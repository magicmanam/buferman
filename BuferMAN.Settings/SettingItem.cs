namespace BuferMAN.Settings
{
    public class SettingItem<T> where T : struct
    {
        public SettingItem(T defaultValue, T savedValue)
        {
            this.DefaultValue = defaultValue;
            this.SavedValue = savedValue;
            this.CurrentValue = savedValue;
        }

        public bool IsDirty
        {
            get
            {
                return !object.Equals(this.CurrentValue, this.SavedValue);
            }
        }
        public bool IsCurrentValueDefault {
            get
            {
                return object.Equals(this.CurrentValue, this.DefaultValue);
            }
        }
        public bool IsSavedValueDefault
        {
            get
            {
                return object.Equals(this.SavedValue, this.DefaultValue);
            }
        }
        public T CurrentValue { get; set; }
        public T SavedValue { get; set; }
        public T DefaultValue { get; }
        public void RestoreDefault()
        {
            this.CurrentValue = this.DefaultValue;
        }
        public SettingItem<T> Save()
        {
            this.SavedValue = this.CurrentValue;

            return this;
        }
    }
}
