using System.Collections.Generic;

namespace BuferMAN.Storage
{
    public interface IPersistentBufersStorage
    {
        IEnumerable<BuferItem> LoadBufers();

        void SaveBufer(BuferItem bufer);
    }
}
