using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BuferMAN.Form.Window
{
    static class IDataObjectExtensions
    {
        public static bool IsEmptyObject(this IDataObject dataObject)
        {
            return !dataObject.GetFormats().Any(f => dataObject.GetData(f) != null);
        }
    }
}
