using System.Windows.Forms;

namespace BuferMAN.Plugins.BuferPresentations
{
    public interface IBuferPresentation
    {
        void ApplyToButton(Button button);
        bool IsCompatibleWithBufer(IDataObject data);
    }
}