using ClipboardBufer;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BuferMAN.ClipPresentations
{
    public class SkypeClipPresentation : IClipPresentation
    {
        private static Image _skypeIcon;

        private static Image SkypeIcon { get
            {
                if (SkypeClipPresentation._skypeIcon == null)
                {
                    SkypeClipPresentation._skypeIcon = Image.FromFile("skype.ico");
                }

                return SkypeClipPresentation._skypeIcon;
            } }
        public bool IsCompatibleWithBufer(IDataObject data)
        {
            return data.GetFormats().Any(format => format == ClipboardFormats.SKYPE_FORMAT);
        }

        public void ApplyToButton(Button button)
        {
            button.Image = SkypeClipPresentation.SkypeIcon;
            button.ImageAlign = ContentAlignment.MiddleRight;
            button.TextAlign = ContentAlignment.MiddleLeft;
        }
    }
}
