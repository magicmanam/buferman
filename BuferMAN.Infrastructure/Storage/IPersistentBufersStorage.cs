using BuferMAN.Models;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure.Storage
{
    public interface IPersistentBufersStorage
    {
        IEnumerable<BuferItem> LoadBufers();

        void SaveBufer(BuferItem bufer);
    }
}
