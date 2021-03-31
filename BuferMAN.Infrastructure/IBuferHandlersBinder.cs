using BuferMAN.View;
using System.Windows.Forms;

namespace BuferMAN.Infrastructure
{
    public interface IBuferHandlersBinder
    {
        void Bind(BuferViewModel buferViewModel, Button button, IBufer bufer, IBuferMANHost buferMANHost);// TODO remove Button parameter (should be IBufer)
    }
}
