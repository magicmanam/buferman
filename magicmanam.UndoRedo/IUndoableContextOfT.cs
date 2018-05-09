using System;

namespace magicmanam.UndoRedo
{
    public interface IUndoableContext<T> where T : class
    {
        event EventHandler<UndoableActionEventArgs> UndoableAction;
        event EventHandler<UndoableActionEventArgs> UndoAction;
        event EventHandler<UndoableActionEventArgs> RedoAction;
        event EventHandler<UndoableContextChangedEventArgs> StateChanged;

        UndoableAction<T> StartAction(string action = null);

        void Undo();
        void Redo();
    }
}
