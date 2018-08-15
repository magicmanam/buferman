using BuferMAN.Clipboard;
using BuferMAN.Form.Window;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Window;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Form
{
    public class DefaultWindowLevelContext : IWindowLevelContext
    {
        private readonly IRenderingHandler _renderingHandler;
        private readonly IWindowHidingHandler _hidingHandler;
        private readonly IWindowActivationHandler _windowActivationHandler;

        public DefaultWindowLevelContext(BuferAMForm form, IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IClipboardWrapper clipboardWrapper, IProgramSettings settings, IFileStorage fileStorage)
        {
            this._renderingHandler = new RenderingHandler(form, clipboardBuferService, comparer, clipboardWrapper, settings, fileStorage);
            this._hidingHandler = new WindowHidingHandler(form);
            this._windowActivationHandler = new WindowActivationHandler(clipboardBuferService, form);
            this.WindowHandle = form.Handle;
        }

        public IntPtr WindowHandle { get; set; }

        public void HideWindow()
        {
            this._hidingHandler.HideWindow();
        }

        public void ActivateWindow()
        {
            this._windowActivationHandler.Activate();
        }

        public void RerenderBufers()
        {
            this._renderingHandler.Render();
        }
    }
}
