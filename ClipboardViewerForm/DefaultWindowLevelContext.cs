using ClipboardViewerForm.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardViewerForm
{
    class DefaultWindowLevelContext : IWindowLevelContext
    {
        private readonly IRenderingHandler _renderingHandler;
        private readonly IWindowHidingHandler _hidingHandler;

        public DefaultWindowLevelContext(IRenderingHandler renderingHandler, IWindowHidingHandler hidingHandler)
        {
            this._renderingHandler = renderingHandler;
            this._hidingHandler = hidingHandler;
        }

        public void HideWindow()
        {
            this._hidingHandler.HideWindow();
        }

        public void RerenderBufers()
        {
            this._renderingHandler.Render();
        }
    }
}
