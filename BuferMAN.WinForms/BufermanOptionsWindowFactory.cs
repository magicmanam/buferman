using BuferMAN.Infrastructure.Environment;
using BuferMAN.Infrastructure.Settings;

namespace BuferMAN.WinForms
{
    internal class BufermanOptionsWindowFactory : IBufermanOptionsWindowFactory
    {
        private readonly IProgramSettingsGetter _settingsGetter;
        private readonly IProgramSettingsSetter _settingsSetter;
        private readonly IUserInteraction _userInteraction;

        public BufermanOptionsWindowFactory(IProgramSettingsGetter settingsGetter,
                                            IProgramSettingsSetter settingsSetter,
                                            IUserInteraction userInteraction)
        {
            this._settingsGetter = settingsGetter;
            this._settingsSetter = settingsSetter;
            this._userInteraction = userInteraction;
        }

        public IBufermanOptionsWindow Create()
        {
            return new BufermanOptionsWindow(
                this._settingsGetter,
                this._settingsSetter,
                this._userInteraction);
        }
    }
}
