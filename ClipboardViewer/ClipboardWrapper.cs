using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardViewer
{
    class ClipboardWrapper : IClipboardWrapper
    {
        public IDataObject GetDataObject()
        {
            try
            {
                return Clipboard.GetDataObject();
            }
            catch (ExternalException exc)
            {
                Logger.Logger.Current.WriteError("An error during get clipboard operation", exc);
                MessageBox.Show("An error occurred. See logs for more details.");
                throw;
            }
        }

        public void SetDataObject(DataObject dataObject)
        {
            Clipboard.SetDataObject(dataObject);
        }
    }
}
