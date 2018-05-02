using BuferMAN.Clipboard;
using BuferMAN.Plugins.BuferPresentations;
using System.Linq;
using System.Windows.Forms;

namespace BuferMAN.BuferPresentations
{
    public class FtpFileBuferPresentation : IconBuferPresentationBase
    {
        public FtpFileBuferPresentation() : base("folder.ico") { }
        
        public override bool IsCompatibleWithBufer(IDataObject data)
        {
            return data.GetFormats().Any(format => format == ClipboardFormats.FTP_FILE_FORMAT);
        }
    }
}
