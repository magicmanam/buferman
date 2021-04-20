using BuferMAN.Assets;
using System.Drawing;
using System.Windows.Forms;

namespace BuferMAN.Windows
{
    public class BufermanGhostForm : Form
    {
        public BufermanGhostForm()
        {
            Icon = Icons.Buferman;
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(0, 0);
            BackColor = Color.White;
            TransparencyKey = Color.White;
        }
    }
}
