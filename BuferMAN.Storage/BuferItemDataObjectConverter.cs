using BuferMAN.Clipboard;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;
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

            if (buferItem.Formats != null)
            {
                foreach (var format in buferItem.Formats) if (format.Value != null)
                {
                    dataObject.SetData(format.Key, format.Value);
                }
            }

            dataObject.SetData(ClipboardFormats.FROM_FILE_FORMAT, null);
            return dataObject;
        }
    }
}
