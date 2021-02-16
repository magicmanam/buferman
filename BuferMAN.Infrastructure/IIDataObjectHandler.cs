using System;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure
{
    public interface IIDataObjectHandler
    {
        event EventHandler Updated;
        void HandleDataObject(IDataObject dataObject);
    }
}