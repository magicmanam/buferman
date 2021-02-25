using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;
using System;

namespace BuferMAN.Files
{
    public class BufersFileStorage : IPersistentBufersStorage
    {
        private readonly ILoadingFileHandler _loadingFileHandler;

        public BufersFileStorage(ILoadingFileHandler loadingFileHandler)
        {
            _loadingFileHandler = loadingFileHandler;
        }

        public void LoadBufers(string fileName)
        {
            this._loadingFileHandler.LoadBufersFromFile(fileName);
        }

        public void LoadBufers()
        {
            throw new NotImplementedException();
        }

        public void SaveBufer(BuferItem bufer)
        {
            throw new NotImplementedException();
        }
    }
}
