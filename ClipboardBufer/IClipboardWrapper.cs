using System.Drawing;
using System.Windows.Forms;

namespace ClipboardBufer
{
    public interface IClipboardWrapper
    {
        IDataObject GetDataObject();
        void SetDataObject(IDataObject dataObject);
    }
}