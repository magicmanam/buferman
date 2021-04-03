using BuferMAN.Infrastructure.Environment;
using System.Windows.Forms;

namespace BuferMAN.Windows
{
    public class UserInteraction : IUserInteraction
    {
        public bool? ShowYesNoCancelPopup(string text, string caption)
        {
            var result = MessageBox.Show(text, caption, MessageBoxButtons.YesNoCancel);

            switch (result)
            {
                case DialogResult.Yes:
                    return true;
                case DialogResult.No:
                    return false;
                default:
                    return null;
            }
        }

        public void ShowPopup(string text, string caption)
        {
            MessageBox.Show(text, caption);
        }
    }
}
