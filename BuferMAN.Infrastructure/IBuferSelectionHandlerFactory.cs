using System.Windows.Forms;

namespace BuferMAN.Infrastructure
{
    public interface IBuferSelectionHandlerFactory
    {
        IBuferSelectionHandler CreateHandler(IBufer bufer, IBufermanHost bufermanHost);
    }
}