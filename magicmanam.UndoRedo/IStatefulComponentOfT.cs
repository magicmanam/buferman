namespace magicmanam.UndoRedo
{
    public interface IStatefulComponent<T>
    {
        T UndoableState { get; set; }
    }
}
