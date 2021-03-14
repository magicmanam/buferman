using System.Windows.Forms;

namespace BuferMAN.Infrastructure.Menu
{
    public interface IMenuGenerator
    {
        MainMenu GenerateMenu(IBuferMANHost buferMANHost);
    }
}
