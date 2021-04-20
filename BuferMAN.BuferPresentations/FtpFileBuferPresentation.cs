using BuferMAN.Clipboard;
using BuferMAN.Plugins.BuferPresentations;
using System.Linq;
using System.Windows.Forms;

namespace BuferMAN.BuferPresentations
{
    public class FtpFileBuferPresentation : FolderIconBuferPresentationBase
    {
        public FtpFileBuferPresentation() { }
        
        public override bool IsCompatibleWithBufer(IDataObject data)
        {
            return data.GetFormats().Any(format => format == ClipboardFormats.FTP_FILE_FORMAT);
        }
    }
}
