namespace magicmanam.UndoRedo
{
    public class UndoableContextChangedEventArgs
    {
        public UndoableContextChangedEventArgs(bool canUndo, bool canRedo)
        {
            CanUndo = canUndo;
            CanRedo = canRedo;
        }

        public bool CanUndo { get; private set; }
        public bool CanRedo { get; private set; }
    }
}