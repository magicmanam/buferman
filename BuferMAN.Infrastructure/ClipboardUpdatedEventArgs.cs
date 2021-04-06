using BuferMAN.View;
using System;

namespace BuferMAN.Infrastructure
{
    public class ClipboardUpdatedEventArgs : EventArgs
    {
        public ClipboardUpdatedEventArgs(BuferViewModel bufer)
        {
            this.Bufer = bufer;
        }

        public BuferViewModel Bufer { get; protected set; }
    }
}
