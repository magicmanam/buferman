using BuferMAN.Models;
using System;

namespace BuferMAN.Infrastructure.Storage
{
    public class BuferLoadedEventArgs : EventArgs
    {
        public BuferLoadedEventArgs(BuferItem bufer)
        {
            this.Bufer = bufer;
        }

        public BuferItem Bufer { get; set; }
    }
}
