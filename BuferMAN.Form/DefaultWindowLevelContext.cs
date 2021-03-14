using BuferMAN.Application;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Window;

namespace BuferMAN.Form
{
    public class DefaultWindowLevelContext : IWindowLevelContext
    {
        private readonly IRenderingHandler _renderingHandler;// TODO remove from here. Use IBuferMANHost.Rerender instead
        private readonly IBuferMANHost _buferManHost;

        public DefaultWindowLevelContext(BuferAMForm form, IRenderingHandler renderingHandler)
        {
            this._renderingHandler = renderingHandler;
            this._buferManHost = form;
        }

        public void HideWindow()
        {
            this._buferManHost.HideWindow();
        }

        public void ActivateWindow()
        {
            this._buferManHost.ActivateWindow();
        }

        public void RerenderBufers()
        {
            this._renderingHandler.Render();
        }
    }
}
