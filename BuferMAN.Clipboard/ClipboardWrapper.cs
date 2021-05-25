using System.Drawing;
using System.Windows.Forms;
using WindowsClipboard = System.Windows.Forms.Clipboard;

namespace BuferMAN.Clipboard
{
    internal class ClipboardWrapper : IClipboardWrapper
    {
        public IDataObject GetDataObject()
        {
            return WindowsClipboard.GetDataObject();
        }

        public void SetDataObject(IDataObject dataObject)
        {
            WindowsClipboard.SetDataObject(dataObject);
        }

        public bool ContainsImage()
        {
            return WindowsClipboard.ContainsImage();
        }

        public Image GetImage()
        {
            return WindowsClipboard.GetImage();
        }
    }
}
