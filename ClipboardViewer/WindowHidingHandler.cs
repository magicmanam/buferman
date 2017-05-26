using System.Windows.Forms;

namespace ClipboardViewer
{
	class WindowHidingHandler
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
