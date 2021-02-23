using BuferMAN.View;
using System;

namespace BuferMAN.Infrastructure
{
    public interface IIDataObjectHandler
    {
        event EventHandler Full;
        event EventHandler Updated;
        bool TryHandleDataObject(BuferViewModel buferViewModel);
    }
}