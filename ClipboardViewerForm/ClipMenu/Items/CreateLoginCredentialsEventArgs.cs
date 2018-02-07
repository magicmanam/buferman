using System;

namespace ClipboardViewerForm.ClipMenu.Items
{
    class CreateLoginCredentialsEventArgs : EventArgs
    {
        public string Password { get; private set; }

        public CreateLoginCredentialsEventArgs(string password)
        {
            this.Password = password;
        }
    }
}
