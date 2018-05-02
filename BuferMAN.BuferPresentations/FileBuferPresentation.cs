using BuferMAN.Plugins.BuferPresentations;
using System.Linq;
using System.Windows.Forms;

namespace BuferMAN.BuferPresentations
{
    public class FileBuferPresentation : IconBuferPresentationBase
    {
        public FileBuferPresentation() : base("folder.ico") { }

        public override bool IsCompatibleWithBufer(IDataObject data)
        {
            return data.GetFormats().Any(format => format == DataFormats.FileDrop);
        }
    }
}
