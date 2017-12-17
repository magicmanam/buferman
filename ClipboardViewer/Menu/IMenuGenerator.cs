using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardViewer.Menu
{
    interface IMenuGenerator
    {
        MainMenu GenerateMenu(Form form);
    }
}
