using ClipboardBufer;
using ClipboardViewerForm.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardViewerForm
{
    public class DefaultWindowLevelContext : IWindowLevelContext
    {
        private readonly IRenderingHandler _renderingHandler;
        private readonly IWindowHidingHandler _hidingHandler;

        public DefaultWindowLevelContext(Form form, IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IClipboardWrapper clipboardWrapper, IDictionary<IDataObject, Button> buttonsMap)
        {
            this._renderingHandler = new RenderingHandler(form, clipboardBuferService, comparer, clipboardWrapper, buttonsMap);
            this._hidingHandler = new WindowHidingHandler(form);
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
