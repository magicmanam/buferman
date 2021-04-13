using System.Drawing;
using System.Windows.Forms;

namespace BuferMAN.Windows
{
    public class BufermanGhostForm : Form
    {
        public BufermanGhostForm()
        {
            Icon = new Icon("copy-multi-size.ico");// TODO (s) new Icon("copy-multi-size.ico") - into a separate class
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(0, 0);
            BackColor = Color.White;
            TransparencyKey = Color.White;
        }
    }
}
