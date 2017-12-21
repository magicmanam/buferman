using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardBufer
{
    public interface IUndoable
    {
        void Undo();
        void CancelUndo();
    }
}
