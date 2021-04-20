using BuferMAN.Files;
using BuferMAN.Infrastructure.Files;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;
using System;

namespace BuferMAN.Storage
{
    internal class BufersStorageFactory : IBufersStorageFactory
    {
        private readonly ILoadedBuferItemsProcessor _loadedBuferItemsProcessor;
        private readonly IBufersFileStorageFactory _filesStorageFactory;

        public BufersStorageFactory(ILoadedBuferItemsProcessor loadedBuferItemsProcessor, IBufersFileStorageFactory filesStorageFactory)
        {
            this._loadedBuferItemsProcessor = loadedBuferItemsProcessor;
            this._filesStorageFactory = filesStorageFactory;
        }

        public IPersistentBufersStorage Create(BufersStorageModel model)
        {
            return this.Create(model.StorageType, model.StorageAddress);
        }

        public IPersistentBufersStorage Create(BufersStorageType storageType, string address)
        {
            IPersistentBufersStorage storage;

            switch (storageType)
            {
                case BufersStorageType.TxtFile:
                case BufersStorageType.JsonFile:
                    storage = this._filesStorageFactory.Create(storageType, address);
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
