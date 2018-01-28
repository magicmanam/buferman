using ClipboardBufer;
using ClipboardViewerForm.Window;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ClipboardViewerForm
{
    public class DefaultWindowLevelContext : IWindowLevelContext
    {
        private readonly IRenderingHandler _renderingHandler;
        private readonly IWindowHidingHandler _hidingHandler;

        public DefaultWindowLevelContext(Form form, IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IClipboardWrapper clipboardWrapper, IDictionary<IDataObject, Button> buttonsMap, IProgramSettings settings)
        {
            this._renderingHandler = new RenderingHandler(form, clipboardBuferService, comparer, clipboardWrapper, buttonsMap, settings);
            this._hidingHandler = new WindowHidingHandler(form);
            this.WindowHandle = form.Handle;
        }

        public IntPtr WindowHandle { get; set; }

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
