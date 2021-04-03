using BuferMAN.View;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure
{
    public interface IBuferHandlersBinder
    {
        void Bind(BuferViewModel buferViewModel, Button button, IBufer bufer, IBufermanHost bufermanHost);// TODO (l) remove Button parameter (should be IBufer)
    }
}
