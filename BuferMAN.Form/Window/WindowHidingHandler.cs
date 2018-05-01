using BuferMAN.Infrastructure.Window;
using System.Windows.Forms;
using SystemWindowsForm = System.Windows.Forms.Form;

namespace BuferMAN.Form.Window
{
	class WindowHidingHandler : IWindowHidingHandler
    {
        private readonly SystemWindowsForm _form;

        public WindowHidingHandler(SystemWindowsForm form)
        {
            this._form = form;
        }

        public void HideWindow()
        {
            this._form.WindowState = FormWindowState.Minimized;
        }
    }
}
