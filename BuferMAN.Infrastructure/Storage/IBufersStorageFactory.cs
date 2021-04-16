using BuferMAN.Models;

namespace BuferMAN.Infrastructure.Storage
{
    public interface IBufersStorageFactory
    {
        IPersistentBufersStorage Create(BufersStorageModel storageModel);
        IPersistentBufersStorage Create(BufersStorageType storageType, string address);
    }
}
