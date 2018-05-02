using BuferMAN.Clipboard;
using BuferMAN.Plugins.BuferPresentations;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BuferMAN.BuferPresentations
{
    public class SkypeBuferPresentation : IconBuferPresentationBase
    {
        public SkypeBuferPresentation() : base("skype.ico") { }

        public override bool IsCompatibleWithBufer(IDataObject data)
        {
            return data.GetFormats().Any(format => format == ClipboardFormats.SKYPE_FORMAT);
        }
    }
}
