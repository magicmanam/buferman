using System;

namespace magicmanam.UndoableOperations
{
    public interface IUndoable
    {
        event EventHandler<UndoableActionEventArgs> UndoableAction;
        event EventHandler<UndoableActionEventArgs> UndoAction;
        event EventHandler<UndoableActionEventArgs> CancelUndoAction;
        event EventHandler<UndoableContextChangedEventArgs> UndoableContextChanged;

        void Undo();
        void CancelUndo();
    }
}
