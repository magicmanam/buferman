using System.Windows.Forms;

namespace ClipboardViewer
{
    interface IClipboardWrapper
    {
        IDataObject GetDataObject();
        void SetDataObject(DataObject dataObject);
    }
}