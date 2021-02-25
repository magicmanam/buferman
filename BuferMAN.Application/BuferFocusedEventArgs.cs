using BuferMAN.View;
using System;

namespace BuferMAN.Application
{
    public class BuferFocusedEventArgs : EventArgs
    {
        public BuferFocusedEventArgs(BuferViewModel bufer)
        {
            this.Bufer = bufer;
        }

        public BuferViewModel Bufer { get; }
    }
}
