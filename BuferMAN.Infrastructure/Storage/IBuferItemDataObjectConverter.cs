using BuferMAN.Models;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure.Storage
{
    public interface IBuferItemDataObjectConverter
    {
        IDataObject ToDataObject(BuferItem buferItem);
    }
}