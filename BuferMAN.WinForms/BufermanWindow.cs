using System;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;

namespace BuferMAN.WinForms
{
	internal partial class BufermanWindow : Form, IBufermanHost
    {
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            WindowsFunctions.UnregisterHotKey(this.Handle, 1);// Разобраться нужно ли это?
            throw new ClipboardMessageException("Do not delete this method if you see this message: BuferAMForm.OnClosed(EventArgs e)", new Exception());
        }
    }
}
