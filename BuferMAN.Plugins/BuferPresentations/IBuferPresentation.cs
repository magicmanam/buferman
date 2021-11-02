using BuferMAN.Infrastructure;
using System.Windows.Forms;

namespace BuferMAN.Plugins.BuferPresentations
{
    public interface IBuferPresentation
    {
        void ApplyToBufer(IBufer button);
        bool IsCompatibleWithBufer(IDataObject data);
    }
}