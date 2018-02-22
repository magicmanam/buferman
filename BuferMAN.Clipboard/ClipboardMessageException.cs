using System;

namespace BuferMAN.Clipboard
{
    public class ClipboardMessageException : Exception
    {
        public ClipboardMessageException(string message, Exception innerException) : base(message, innerException) { }
    }
}
