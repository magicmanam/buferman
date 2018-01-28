using System;

namespace ClipboardViewerForm
{
    interface ILoadingFileHandler
    {
        void OnLoadFile(object sender, EventArgs args);
        void LoadBufersFromFile(string fileName);
    }
}
