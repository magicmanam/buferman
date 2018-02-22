using System.Windows.Forms;

namespace BuferMAN.ClipPresentations
{
    public interface IClipPresentation
    {
        void ApplyToButton(Button button);
        bool IsCompatibleWithBufer(IDataObject data);
    }
}