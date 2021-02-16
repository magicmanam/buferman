using BuferMAN.Infrastructure.Storage;
using System;
using System.Collections.Generic;

namespace BuferMAN.Files
{
    public class BufersFileStorage : IPersistentBufersStorage
    {
        private readonly ILoadingFileHandler _loadingFileHandler;

        public BufersFileStorage(ILoadingFileHandler loadingFileHandler)
        {
            _loadingFileHandler = loadingFileHandler;
        }

        public IEnumerable<BuferItem> LoadBufers(string fileName)
        {
            return this._loadingFileHandler.LoadBufersFromFile(fileName);
        }

        public IEnumerable<BuferItem> LoadBufers()
        {
            throw new NotImplementedException();
        }

        public void SaveBufer(BuferItem bufer)
        {
            throw new NotImplementedException();
        }
    }
}
