using System.Windows.Forms;

namespace BuferMAN.Infrastructure
{
    public interface IIDataObjectHandler
    {
        void HandleDataObject(IDataObject dataObject);
    }
}