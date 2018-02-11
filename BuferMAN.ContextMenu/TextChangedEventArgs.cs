namespace BuferMAN.ContextMenu
{
    public class TextChangedEventArgs
    {
        public bool IsOriginText { get; private set; }
        public TextChangedEventArgs(bool isOriginText)
        {
            this.IsOriginText = isOriginText;
        }
    }
}
