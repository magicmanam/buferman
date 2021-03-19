using System.Windows.Forms;

namespace BuferMAN.Infrastructure.Window
{
    public interface IRenderingHandler
    {
        void Render();

        void SetForm(Form form);// TODO remove this method from here
    }
}