using BuferMAN.Clipboard;
using BuferMAN.Form.Window;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Window;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SystemWindowsForm = System.Windows.Forms.Form;

namespace BuferMAN.Form
{
    public class DefaultWindowLevelContext : IWindowLevelContext
    {
        private readonly IRenderingHandler _renderingHandler;
        private readonly IWindowHidingHandler _hidingHandler;

        public DefaultWindowLevelContext(BuferAMForm form, IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IClipboardWrapper clipboardWrapper, IProgramSettings settings)
        {
            this._renderingHandler = new RenderingHandler(form, clipboardBuferService, comparer, clipboardWrapper, settings);
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
