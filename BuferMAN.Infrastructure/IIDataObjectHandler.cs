using BuferMAN.View;
using System;

namespace BuferMAN.Infrastructure
{
    public interface IIDataObjectHandler
    {
        event EventHandler Full;
        event EventHandler<ClipboardUpdatedEventArgs> Updated;
        bool TryHandleDataObject(BuferViewModel buferViewModel);
        long CopiesCount { get; }
        long CurrentDayCopiesCount { get; }
    }
}