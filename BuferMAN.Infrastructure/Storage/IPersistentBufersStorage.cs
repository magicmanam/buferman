using BuferMAN.Models;
using System;

namespace BuferMAN.Infrastructure.Storage
{
    public interface IPersistentBufersStorage
    {
        event EventHandler<BufersLoadedEventArgs> BufersLoaded;

        void LoadBufers();

        void SaveBufer(BuferItem buferItem);
    }
}
