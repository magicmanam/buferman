using System.Runtime.InteropServices;
using Logging;
using System.Windows.Forms;
using WindowsClipboard = System.Windows.Forms.Clipboard;

namespace BuferMAN.Clipboard
{
    internal class ClipboardWrapper : IClipboardWrapper
    {
        public IDataObject GetDataObject()
        {
            try
            {
                var dataObject = WindowsClipboard.GetDataObject();

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

                if (WindowsClipboard.ContainsImage())
                {
                    copy.SetData(ClipboardFormats.CUSTOM_IMAGE_FORMAT, WindowsClipboard.GetImage());
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
            WindowsClipboard.SetDataObject(dataObject);
        }
    }
}
