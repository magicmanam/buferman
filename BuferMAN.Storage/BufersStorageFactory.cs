using BuferMAN.Files;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;
using System;

namespace BuferMAN.Storage
{
    public class BufersStorageFactory : IBufersStorageFactory
    {
        private readonly ILoadedBuferItemsProcessor _loadedBuferItemsProcessor;

        public BufersStorageFactory(ILoadedBuferItemsProcessor loadedBuferItemsProcessor)
        {
            this._loadedBuferItemsProcessor = loadedBuferItemsProcessor;
        }

        public IPersistentBufersStorage Create(BufersStorageModel model)
        {
            return this.Create(model.StorageType, model.StorageAddress);
        }

        public IPersistentBufersStorage Create(BufersStorageType storageType, string address)
        {
            BufersFileStorage storage;

            switch (storageType)
            {
                case BufersStorageType.TxtFile:
                    storage = new BufersFileStorage(new TxtFileFormatter(), address);
                    break;
                case BufersStorageType.JsonFile:
                    storage = new BufersFileStorage(new JsonFileFormatter(), address);
                    break;
                default:
                    throw new NotImplementedException();
            }

            storage.BufersLoaded += (object sender, BufersLoadedEventArgs args) => {
                this._loadedBuferItemsProcessor.ProcessBuferItems(args.Bufers);
            };

            return storage;
        }
    }
}
