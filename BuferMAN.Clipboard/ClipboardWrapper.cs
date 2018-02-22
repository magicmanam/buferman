using System.Runtime.InteropServices;
using Logging;
using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
    public class ClipboardWrapper : IClipboardWrapper
    {
        public IDataObject GetDataObject()
        {
            try
            {
                IDataObject dataObject = System.Windows.Forms.Clipboard.GetDataObject();

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

                if (System.Windows.Forms.Clipboard.ContainsImage())
                {
                    copy.SetData(ClipboardFormats.CUSTOM_IMAGE_FORMAT, System.Windows.Forms.Clipboard.GetImage());
                }

                return copy;
            }
            catch (ExternalException exc)
            {
                Logger.WriteError("An error during get clipboard operation", exc);
                throw new ClipboardMessageException("An error occurred. See logs for more details.", exc);
            }
        }

        public void SetDataObject(IDataObject dataObject)
        {
            System.Windows.Forms.Clipboard.SetDataObject(dataObject);
        }
    }
}
