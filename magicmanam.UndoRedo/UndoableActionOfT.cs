using System;

namespace magicmanam.UndoRedo
{
    public class UndoableAction<T> : IDisposable where T : class
    {
        private readonly UndoableContext<T> _context;

        internal UndoableAction(UndoableContext<T> context)
        {
            this._context = context;
        }

        public bool IsCancelled { get; private set; }
        public void Dispose()
        {
            this._context.EndAction(this.IsCancelled);
        }

        public void Cancel() {
            this.IsCancelled = true;
        }
    }
}
