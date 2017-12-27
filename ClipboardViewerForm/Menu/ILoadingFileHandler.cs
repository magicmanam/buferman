using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardViewerForm.Menu
{
    interface ILoadingFileHandler
    {
        void OnLoadFile(object sender, EventArgs args);
    }
}
