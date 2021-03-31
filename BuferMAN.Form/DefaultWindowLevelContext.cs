using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Window;

namespace BuferMAN.Form
{
    public class DefaultWindowLevelContext : IWindowLevelContext
    {
        private readonly IRenderingHandler _renderingHandler;// TODO remove from here. Use IBuferMANHost.Rerender instead
        private readonly IBuferMANHost _buferMANHost;

        public DefaultWindowLevelContext(IBuferMANHost buferManHost, IRenderingHandler renderingHandler)
        {
            this._renderingHandler = renderingHandler;
            this._buferMANHost = buferManHost;
        }

        public void HideWindow()
        {
            this._buferMANHost.HideWindow();
        }

        public void ActivateWindow()
        {
            this._buferMANHost.ActivateWindow();
        }

        public void RerenderBufers()
        {
            this._renderingHandler.Render(this._buferMANHost);
        }
    }
}
