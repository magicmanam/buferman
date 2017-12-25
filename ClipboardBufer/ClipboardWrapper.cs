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
                IDataObject dataObject = Clipboard.GetDataObject();

                var copy = new DataObject();
                foreach (var format in dataObject.GetFormats())
                {
                    if (format == "EnhancedMetafile")//Fixes bug with copy in Word
                    {
                        copy.SetData(format, "<< !!! EnhancedMetafile !!! >>");
                    }
                    else
                    {
                        try
                        {
                            copy.SetData(format, dataObject.GetData(format));
                        }
                        catch
                        {
                            //Log input parameters and other details.
                        }
                    }
                }

                if (Clipboard.ContainsImage())
                {
                    copy.SetData(ClipboardFormats.CUSTOM_IMAGE_FORMAT, Clipboard.GetImage());
                }

                return copy;
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
