using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardViewerForm
{
    interface IWindowLevelContext
    {
        void HideWindow();
        void RerenderBufers();
    }
}
