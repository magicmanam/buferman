using BuferMAN.Models;
using System;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure.Storage
{
    public interface IPersistentBufersStorage
    {
        event EventHandler<BufersLoadedEventArgs> BufersLoaded;

        void LoadBufers();

        void SaveBufer(BuferItem buferItem);

        void SaveBufers(IEnumerable<BuferItem> buferItems);
    }
}
