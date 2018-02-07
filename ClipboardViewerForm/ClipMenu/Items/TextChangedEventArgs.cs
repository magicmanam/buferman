using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardViewerForm.ClipMenu.Items
{
    public class TextChangedEventArgs
    {
        public bool IsOriginText { get; private set; }
        public TextChangedEventArgs(bool isOriginText)
        {
            this.IsOriginText = isOriginText;
        }
    }
}
