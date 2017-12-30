using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardBufer
{
    public class UndoableActionEventArgs
    {
        public UndoableActionEventArgs(string action)
        {
            Action = action;
        }

        public string Action { get; private set; }
    }
}
