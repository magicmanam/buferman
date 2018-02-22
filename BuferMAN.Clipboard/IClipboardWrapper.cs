using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
    public interface IClipboardWrapper
    {
        IDataObject GetDataObject();
        void SetDataObject(IDataObject dataObject);
    }
}