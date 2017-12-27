using System.Windows.Forms;

namespace ClipboardViewerForm.Window
{
	class WindowHidingHandler : IWindowHidingHandler
    {
        private readonly Form _form;

        public WindowHidingHandler(Form form)
        {
            _form = form;
        }

        public void HideWindow()
        {
            this._form.WindowState = FormWindowState.Minimized;
        }
    }
}
