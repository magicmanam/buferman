using System.Linq;
using System.Windows.Forms;

namespace BuferMAN.WinForms.Window
{
    static class IDataObjectExtensions
    {
        public static bool IsEmptyObject(this IDataObject dataObject)
        {
            return !dataObject.GetFormats().Any(f => dataObject.GetData(f) != null);
        }
    }
}
