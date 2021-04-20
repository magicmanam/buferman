using BuferMAN.Infrastructure.Files;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;
using System;

namespace BuferMAN.Files
{
    internal class BufersFileStorageFactory : IBufersFileStorageFactory
    {
        public BufersFileStorageFactory()
        {
        }

        public IPersistentBufersStorage Create(BufersStorageModel model)
        {
            return this.Create(model.StorageType, model.StorageAddress);
        }

        public IPersistentBufersStorage Create(BufersStorageType storageType, string address)
        {
            switch (storageType)
            {
                case BufersStorageType.TxtFile:
                    return new BufersFileStorage(new TxtFileFormatter(), address);
                case BufersStorageType.JsonFile:
                    return new BufersFileStorage(new JsonFileFormatter(), address);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
