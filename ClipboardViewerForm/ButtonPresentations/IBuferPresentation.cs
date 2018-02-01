using System.Windows.Forms;

namespace ClipboardViewerForm.ButtonPresentations
{
    public interface IBuferPresentation
    {
        void ApplyToButton(Button button);
        bool IsCompatibleWithBufer(IDataObject data);
    }
}