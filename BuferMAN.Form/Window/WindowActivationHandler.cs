using System.Windows.Forms;
using BuferMAN.Clipboard;
using BuferMAN.Infrastructure.Window;
using SystemWindowsForm = System.Windows.Forms.Form;

namespace BuferMAN.Form.Window
{
	class WindowActivationHandler : IWindowActivationHandler
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly SystemWindowsForm _form;

        public WindowActivationHandler(IClipboardBuferService clipboadService, SystemWindowsForm form)
        {
            _clipboardBuferService = clipboadService;
            _form = form;

        }

        public void Activate()
        {
            this._form.WindowState = FormWindowState.Normal;
            this._form.Visible = true;
        }
    }
}
