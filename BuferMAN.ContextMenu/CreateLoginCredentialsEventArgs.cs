using System;

namespace BuferMAN.ContextMenu
{
    public class CreateLoginCredentialsEventArgs : EventArgs
    {
        public string Password { get; private set; }

        public CreateLoginCredentialsEventArgs(string password)
        {
            this.Password = password;
        }
    }
}
