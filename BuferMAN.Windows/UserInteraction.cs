using BuferMAN.Infrastructure.Environment;
using System;
using System.Windows.Forms;

namespace BuferMAN.Windows
{
    public class UserInteraction : IUserInteraction
    {
        public bool? ShowYesNoCancelPopup(string text, string caption)
        {
            var result = this._ShowDialogBoxWithBufermanIcon(() =>
            {
                return MessageBox.Show(text, caption, MessageBoxButtons.YesNoCancel);
            });

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
            this._ShowDialogBoxWithBufermanIcon(() =>
            {
                return MessageBox.Show(text, caption);
            });
        }

        private DialogResult _ShowDialogBoxWithBufermanIcon(Func<DialogResult> dialogFunc)
        {
            using (var ghostForm = new BufermanGhostForm())
            {
                ghostForm.Show();

                return dialogFunc();
            }
        }
    }
}
