using System;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure.Storage
{
    public interface ILoadingFileHandler
    {
        event EventHandler<BuferLoadedEventArgs> BuferLoaded;
        void OnLoadFile(object sender, EventArgs args);
        IEnumerable<BuferItem> LoadBufersFromFile(string fileName);
    }
}
