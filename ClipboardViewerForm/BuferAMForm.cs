using System;
using System.Windows.Forms;
using Windows;

namespace ClipboardViewerForm
{
	public partial class BuferAMForm : Form
    {
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            WindowsFunctions.UnregisterHotKey(this.Handle, 1);
        }
    }
}
