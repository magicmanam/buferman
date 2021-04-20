using BuferMAN.Assets;
using System.Drawing;
using System.Windows.Forms;

namespace BuferMAN.Windows
{
    public class BufermanGhostForm : Form
    {
        public BufermanGhostForm()
        {
            this.Icon = Icons.Buferman;
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(0, 0);
            this.BackColor = Color.White;
            this.TransparencyKey = Color.White;
        }
    }
}
