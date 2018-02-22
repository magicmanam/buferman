using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BuferMAN.ClipPresentations
{
    public class FileClipPresentation : IClipPresentation
    {
        private static Image _fileIcon;

        private static Image FileIcon
        {
            get
            {
                if (FileClipPresentation._fileIcon == null)
                {
                    FileClipPresentation._fileIcon = Image.FromFile("folder.ico");
                }

                return FileClipPresentation._fileIcon;
            }
        }
        public void ApplyToButton(Button button)
        {
            button.Image = FileClipPresentation.FileIcon;
            button.ImageAlign = ContentAlignment.MiddleRight;
            button.TextAlign = ContentAlignment.MiddleLeft;
            //button.BackColor = Color.Khaki;
        }

        public bool IsCompatibleWithBufer(IDataObject data)
        {
            return data.GetFormats().Any(format => format == DataFormats.FileDrop);
        }
    }
}
