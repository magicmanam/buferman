using System;

namespace BuferMAN.ContextMenu
{
    internal class CreateLoginCredentialsEventArgs : EventArgs
    {
        public string Password { get; private set; }

        public CreateLoginCredentialsEventArgs(string password)
        {
            this.Password = password;
        }
    }
}
