using System;
using System.Windows.Forms;
using ClipboardBufer;
using BuferMAN.Infrastructure;

namespace ClipboardViewerForm.Window
{
	class WindowActivationHandler : IWindowActivationHandler
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly Form _form;

        public WindowActivationHandler(IClipboardBuferService clipboadService, Form form)
        {
            _clipboardBuferService = clipboadService;
            _form = form;

        }
                
        public void OnActivated(object sender, EventArgs e)
        {
			this._form.WindowState = FormWindowState.Normal;
            this._form.Visible = true;

            WindowLevelContext.Current.RerenderBufers();
        }
    }
}
