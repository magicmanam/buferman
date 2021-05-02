using System;

namespace BuferMAN.Settings
{
    public class SettingItem<T> : ISettingItem where T : struct
    {
        private readonly Action<T> _saveFn;

        public SettingItem(T defaultValue, T savedValue, Action<T> saveFn)
        {
            this.DefaultValue = defaultValue;
            this.SavedValue = savedValue;
            this.CurrentValue = savedValue;

            this._saveFn = saveFn;
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
        public void Save()
        {
            this.SavedValue = this.CurrentValue;
            this._saveFn(this.SavedValue);
        }
    }
}
