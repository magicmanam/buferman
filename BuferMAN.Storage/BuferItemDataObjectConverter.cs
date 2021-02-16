using BuferMAN.Clipboard;
using BuferMAN.Infrastructure.Storage;
using System.Windows.Forms;

namespace BuferMAN.Storage
{
    public class BuferItemDataObjectConverter : IBuferItemDataObjectConverter
    {
        public IDataObject ToDataObject(BuferItem buferItem)
        {
            var dataObject = new DataObject();

            if (!string.IsNullOrWhiteSpace(buferItem.Text))
            {
                dataObject.SetText(buferItem.Text);
            }

            dataObject.SetData(ClipboardFormats.FROM_FILE_FORMAT, null);
            return dataObject;
        }
    }
}
