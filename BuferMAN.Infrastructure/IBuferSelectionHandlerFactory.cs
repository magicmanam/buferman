using System.Windows.Forms;

namespace BuferMAN.Infrastructure
{
    public interface IBuferSelectionHandlerFactory
    {
        IBuferSelectionHandler CreateHandler(IDataObject dataObject);
    }
}