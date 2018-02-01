using ClipboardBufer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardViewerForm.ButtonPresentations
{
    public class SkypeBuferPresentation : IBuferPresentation
    {
        private static Image _skypeIcon;

        private static Image SkypeIcon { get
            {
                if (SkypeBuferPresentation._skypeIcon == null)
                {
                    SkypeBuferPresentation._skypeIcon = Image.FromFile("skype.ico");
                }

                return SkypeBuferPresentation._skypeIcon;
            } }
        public bool IsCompatibleWithBufer(IDataObject data)
        {
            return data.GetFormats().Any(format => format == ClipboardFormats.SKYPE_FORMAT);
        }

        public void ApplyToButton(Button button)
        {
            button.Image = SkypeBuferPresentation.SkypeIcon;
            button.ImageAlign = ContentAlignment.MiddleRight;
            button.TextAlign = ContentAlignment.MiddleLeft;
        }
    }
}
