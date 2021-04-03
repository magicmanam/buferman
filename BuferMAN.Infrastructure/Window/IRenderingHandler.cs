using System.Windows.Forms;

namespace BuferMAN.Infrastructure.Window
{
    public interface IRenderingHandler
    {
        void Render(IBufermanHost bufermanHost);

        void SetForm(Form form);// TODO (l) remove this method from here
    }
}