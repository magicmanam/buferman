using System.Linq;
using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
    public static class IDataObjectExtensions
    {
        public static bool IsEmptyObject(this IDataObject dataObject)
        {
            return !dataObject.GetFormats().Any(f => dataObject.GetData(f) != null);
        }

        public static bool IsTextObject(this IDataObject dataObject)
        {
            return dataObject.GetFormats().Any(f => ClipboardFormats.TextFormats.Any(tf => tf == f));
        }
    }
}
