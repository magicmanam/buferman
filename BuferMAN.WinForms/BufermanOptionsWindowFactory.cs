using BuferMAN.Infrastructure.Settings;

namespace BuferMAN.WinForms
{
    internal class BufermanOptionsWindowFactory : IBufermanOptionsWindowFactory
    {
        private readonly IProgramSettings _programSettings;

        public BufermanOptionsWindowFactory(IProgramSettings programSettings)
        {
            this._programSettings = programSettings;
        }

        public IBufermanOptionsWindow Create()
        {
            return new BufermanOptionsWindow(this._programSettings);
        }
    }
}
