using System;
using BuferMAN.Application;
using magicmanam.Windows;
using BuferMAN.Clipboard;

namespace BuferMAN.Form
{
	public partial class BuferAMForm : System.Windows.Forms.Form, IBuferMANHost
    {
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            WindowsFunctions.UnregisterHotKey(this.Handle, 1);// Разобраться нужно ли это?
            throw new ClipboardMessageException("Do not delete this method if you see this message", new Exception());
        }
    }
}
