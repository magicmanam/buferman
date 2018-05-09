using magicmanam.UndoRedo.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace magicmanam.UndoRedo
{
    public class UndoableContext<T> : IUndoableContext<T> where T : class
    {
        public event EventHandler<UndoableActionEventArgs> UndoableAction;
        public event EventHandler<UndoableActionEventArgs> UndoAction;
        public event EventHandler<UndoableActionEventArgs> RedoAction;
        public event EventHandler<UndoableContextChangedEventArgs> StateChanged;

        private readonly Stack<T> _states = new Stack<T>();
        private readonly Stack<T> _undoableStates = new Stack<T>();
        private T _stateOnStartAction;
        private IStatefulComponent<T> _stateKeeper;
        private int _actionCount = 0;
        private string _actionName;

        public UndoableContext(IStatefulComponent<T> stateKeeper)
        {
            this._stateKeeper = stateKeeper;
        }

        public static IUndoableContext<T> Current { get; set; }

        public UndoableAction<T> StartAction(string action = null) {
            this._actionName = action;

            if (this._actionCount++ == 0)
            {
                this._stateOnStartAction = this._stateKeeper.UndoableState;
            }

            return new UndoableAction<T>(this);
        }

        internal void EndAction(bool cancelled)
        {
            if (--this._actionCount == 0)
            {
                if (!cancelled)
                {
                    this._states.Push(this._stateOnStartAction);
                    this.OnUndoableAction();
                }

                this._stateOnStartAction = null;
            }
        }

        protected virtual void OnUndoableAction()
        {
            this._undoableStates.Clear();
            this.UndoableAction?.Invoke(this, new UndoableActionEventArgs(this._actionName));
            this.StateChanged?.Invoke(this, new UndoableContextChangedEventArgs(true, false));
        }

        public void Undo()
        {
            if (this._states.Count > 0)
            {
                this._undoableStates.Push(this._stateKeeper.UndoableState);

                var lastState = this._states.Pop();
                this._stateKeeper.UndoableState = lastState;
                this.UndoAction?.Invoke(this, new UndoableActionEventArgs(Resource.BuferOperationCancelled));
                this.StateChanged?.Invoke(this, new UndoableContextChangedEventArgs(this._states.Any(), true));
            }
        }
        public void Redo()
        {
            if (this._undoableStates.Count > 0)
            {
                this._states.Push(this._stateKeeper.UndoableState);

                var undoState = this._undoableStates.Pop();
                this._stateKeeper.UndoableState = undoState;
                this.RedoAction?.Invoke(this, new UndoableActionEventArgs(Resource.BuferOperationRestored));
                this.StateChanged?.Invoke(this, new UndoableContextChangedEventArgs(true, this._undoableStates.Any()));
            }
        }
    }
}
