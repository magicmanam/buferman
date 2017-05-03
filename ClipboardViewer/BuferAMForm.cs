using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardViewer
{
    public partial class BuferAMForm : Form
    {
        private IntPtr _nextViewer;
        
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            UnregisterHotKey(this.Handle, 0);
        }
    }
}
