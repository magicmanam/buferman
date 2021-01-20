using BuferMAN.Clipboard;
using BuferMAN.Plugins.BuferPresentations;
using System.Linq;
using System.Windows.Forms;

namespace BuferMAN.BuferPresentations
{
    public class FileContentsBuferPresentation : IconBuferPresentationBase
    {
        public FileContentsBuferPresentation() : base("folder.ico") { }

        public override bool IsCompatibleWithBufer(IDataObject data)
        {
            if (data.GetFormats().Any(format => format == ClipboardFormats.FILE_CONTENTS_FORMAT))
            {
                return true;
            }

            var files = data.GetData(DataFormats.FileDrop) as string[];
            if (files != null)
            {
                return true;
            }

            return false;
        }
    }
}
