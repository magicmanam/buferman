using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardBufer
{
    public interface IUndoable
    {
        event EventHandler<UndoableActionEventArgs> UndoableAction;
        event EventHandler<UndoableActionEventArgs> UndoAction;
        event EventHandler<UndoableActionEventArgs> CancelUndoAction;
        void Undo();
        void CancelUndo();
    }
}
