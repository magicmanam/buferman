using BuferMAN.View;
using System;

namespace BuferMAN.Infrastructure
{
    public interface IIDataObjectHandler
    {
        event EventHandler Updated;
        void HandleDataObject(BuferViewModel buferViewModel);
    }
}