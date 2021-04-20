using BuferMAN.Infrastructure;

namespace BuferMAN.Application
{
    internal class DefaultWindowLevelContext : IWindowLevelContext
    {
        private readonly IBufermanHost _bufermanHost;

        public DefaultWindowLevelContext(IBufermanHost buferManHost)
        {
            this._bufermanHost = buferManHost;
        }

        public void HideWindow()
        {
            this._bufermanHost.HideWindow();
        }
    }
}
