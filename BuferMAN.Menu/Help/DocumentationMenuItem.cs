using BuferMAN.Menu.Properties;
using System.Diagnostics;
using System.Windows.Forms;

namespace BuferMAN.Menu.Help
{
    public class DocumentationMenuItem : MenuItem
    {
        public DocumentationMenuItem()
        {
            this.Text = Resource.DocumentationMenuItem;
            this.Click += this._DocumentationMenuItem_Click;
        }

        private void _DocumentationMenuItem_Click(object sender, System.EventArgs e)
        {
            Process.Start("Documentation.html");
        }
    }
}