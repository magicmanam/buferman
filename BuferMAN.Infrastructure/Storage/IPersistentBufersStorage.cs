using BuferMAN.Models;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure.Storage
{
    public interface IPersistentBufersStorage
    {
        void LoadBufers();

        void SaveBufer(BuferItem bufer);
    }
}
