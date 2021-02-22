using BuferMAN.Models;
using System;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure.Storage
{
    public class BufersLoadedEventArgs : EventArgs
    {
        public BufersLoadedEventArgs(IEnumerable<BuferItem> bufers)
        {
            this.Bufers = bufers;
        }

        public IEnumerable<BuferItem> Bufers { get; set; }
    }
}
