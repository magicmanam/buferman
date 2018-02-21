using BuferMAN.Menu.Properties;
using System.Diagnostics;
using System.Windows.Forms;

namespace BuferMAN.Menu.Help
{
    public class SendFeedbackMenuItem : MenuItem
    {
        public SendFeedbackMenuItem()
        {
            this.Text = Resource.MenuHelpSend;
            this.Click += this._SendFeedbackMenuItem_Click;
        }

        private void _SendFeedbackMenuItem_Click(object sender, System.EventArgs e)
        {
            Process.Start("https://rink.hockeyapp.net/apps/51633746a31f44999eca3bc7b7945e92/feedback/new");
        }
    }
}
