using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClipboardViewerForm.ButtonPresentations
{
    class FileButtonPresentation : IBuferPresentation
    {
        private static Image _fileIcon;

        private static Image FileIcon
        {
            get
            {
                if (FileButtonPresentation._fileIcon == null)
                {
                    FileButtonPresentation._fileIcon = Image.FromFile("folder.ico");
                }

                return FileButtonPresentation._fileIcon;
            }
        }
        public void ApplyToButton(Button button)
        {
            button.Image = FileButtonPresentation.FileIcon;
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
