using System;

namespace BuferMAN.Infrastructure
{
    public interface ILoadingFileHandler
    {
        void OnLoadFile(object sender, EventArgs args);
        void LoadBufersFromFile(string fileName);
    }
}
