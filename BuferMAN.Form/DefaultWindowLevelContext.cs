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

        public DefaultWindowLevelContext(SystemWindowsForm form, IClipboardBuferService clipboardBuferService, IEqualityComparer<IDataObject> comparer, IClipboardWrapper clipboardWrapper, IDictionary<IDataObject, Button> buttonsMap, IProgramSettings settings)
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
