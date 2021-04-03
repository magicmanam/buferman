using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Window;

namespace BuferMAN.Form
{
    public class DefaultWindowLevelContext : IWindowLevelContext
    {
        private readonly IRenderingHandler _renderingHandler;// TODO (m) remove from here. Use IBuferMANHost.Rerender instead
        private readonly IBufermanHost _bufermanHost;

        public DefaultWindowLevelContext(IBufermanHost buferManHost, IRenderingHandler renderingHandler)
        {
            this._renderingHandler = renderingHandler;
            this._bufermanHost = buferManHost;
        }

        public void HideWindow()
        {
            this._bufermanHost.HideWindow();
        }

        public void ActivateWindow()
        {
            this._bufermanHost.ActivateWindow();
        }

        public void RerenderBufers()
        {
            this._renderingHandler.Render(this._bufermanHost);
        }
    }
}
