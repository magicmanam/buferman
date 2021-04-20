using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;

namespace BuferMAN.Infrastructure.Files
{
    public interface IBufersFileStorageFactory
    {
        IPersistentBufersStorage Create(BufersStorageType storageType, string address);
    }
}
