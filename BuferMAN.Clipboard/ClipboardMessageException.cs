using System;

namespace BuferMAN.Clipboard
{
    public class ClipboardMessageException : Exception
    {
        public string Title { get; set; }
        public ClipboardMessageException(string message, Exception innerException) : base(message, innerException) { }
    }
}
