using BuferMAN.Models;
using System;
using System.Collections.Generic;

namespace BuferMAN.Infrastructure.Storage
{
    public interface ILoadingFileHandler
    {
        event EventHandler<BufersLoadedEventArgs> BufersLoaded;
        void OnLoadFile(object sender, EventArgs args);
        void LoadBufersFromFile(string fileName);
    }
}
