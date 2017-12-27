using System;
using System.Windows.Forms;
using ClipboardBufer;
using Logging;

namespace ClipboardViewerForm.Window
{
	class WindowActivationHandler : IWindowActivationHandler
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly Form _form;

        private readonly IRenderingHandler _renderingHandler;

        public WindowActivationHandler(IClipboardBuferService clipboadService, Form form, IRenderingHandler renderingHandler)
        {
            _clipboardBuferService = clipboadService;
            _form = form;
            _renderingHandler = renderingHandler;

        }
                
        public void OnActivated(object sender, EventArgs e)
        {
			Logger.Write("On Activated");
			this._form.WindowState = FormWindowState.Normal;
            this._form.Visible = true;

            this._renderingHandler.Render();
        }        
    }
}
