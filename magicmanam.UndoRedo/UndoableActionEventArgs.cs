namespace magicmanam.UndoRedo
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
