using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardViewer
{
    interface ILoadingFileHandler
    {
        void OnLoadFile(object sender, EventArgs args);
    }
}
