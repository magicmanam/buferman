using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Logging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace ClipboardBufer
{
    public class ClipboardWrapper : IClipboardWrapper
    {
        public IDataObject GetDataObject()
        {
            try
            {
                return Clipboard.GetDataObject();
            }
            catch (ExternalException exc)
            {
                Logger.WriteError("An error during get clipboard operation", exc);
                MessageBox.Show("An error occurred. See logs for more details.");
                throw;
            }
        }

        public Image GetImage()
        {
            try
            {
                return Clipboard.ContainsImage() ? Clipboard.GetImage() : null;
            }
            catch (ExternalException exc)
            {
                Logger.WriteError("An error during get clipboard operation", exc);
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
