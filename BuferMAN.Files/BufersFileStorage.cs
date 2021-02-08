using BuferMAN.Infrastructure;
using BuferMAN.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
