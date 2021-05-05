using BuferMAN.Infrastructure.Files;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;
using System;

namespace BuferMAN.Files
{
    internal class BufersFileStorageFactory : IBufersFileStorageFactory
    {
        private readonly IFileStorage _fileStorage;

        public BufersFileStorageFactory(IFileStorage fileStorage)
        {
            this._fileStorage = fileStorage;
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
                    return new BufersFileStorage(new TxtFileFormatter(), address, this._fileStorage);
                case BufersStorageType.JsonFile:
                    return new BufersFileStorage(new JsonFileFormatter(), address, this._fileStorage);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
