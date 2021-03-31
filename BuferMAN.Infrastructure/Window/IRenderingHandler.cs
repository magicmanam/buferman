using System.Windows.Forms;

namespace BuferMAN.Infrastructure.Window
{
    public interface IRenderingHandler
    {
        void Render(IBuferMANHost buferMANHost);

        void SetForm(Form form);// TODO remove this method from here
    }
}