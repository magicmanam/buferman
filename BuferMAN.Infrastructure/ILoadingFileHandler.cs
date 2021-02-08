using BuferMAN.Storage;
using System;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure
{
    public interface ILoadingFileHandler
    {
        void OnLoadFile(object sender, EventArgs args);
        IEnumerable<BuferItem> LoadBufersFromFile(string fileName);
    }
}
