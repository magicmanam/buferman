using System.Diagnostics;
using System.Windows.Forms;

namespace BuferMAN.Menu.Help
{
    public class KlopatMenuItem : MenuItem
    {
        public KlopatMenuItem()
        {
            this.Text = "-> klopat.by";
            this.Click += this._KlopatMenuItem_Click;
        }

        private void _KlopatMenuItem_Click(object sender, System.EventArgs e)
        {
            Process.Start("http://www.klopat.by/");
        }
    }
}