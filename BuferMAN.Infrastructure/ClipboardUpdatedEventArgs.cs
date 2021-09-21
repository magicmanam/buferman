using BuferMAN.View;
using System;

namespace BuferMAN.Infrastructure
{
    public class ClipboardUpdatedEventArgs : EventArgs
    {
        public ClipboardUpdatedEventArgs(BuferViewModel bufer)
        {
            this.ViewModel = bufer;
        }

        public BuferViewModel ViewModel { get; protected set; }
    }
}
