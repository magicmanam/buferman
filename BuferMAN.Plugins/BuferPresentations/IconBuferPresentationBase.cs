using BuferMAN.Infrastructure;
using System.Drawing;
using System.Windows.Forms;

namespace BuferMAN.Plugins.BuferPresentations
{
    public abstract class IconBuferPresentationBase : IBuferPresentation
    {
        private string _iconPath;
        private Image _fileIcon;

        public IconBuferPresentationBase(string iconPath)
        {
            this._iconPath = iconPath;
        }

        protected Image FileIcon
        {
            get
            {
                if (this._fileIcon == null)
                {
                    this._fileIcon = Image.FromFile(this._iconPath);
                }

                return this._fileIcon;
            }
        }
        public virtual void ApplyToBufer(IBufer bufer)
        {
            bufer.Image = this.FileIcon;
            bufer.ImageAlign = ContentAlignment.MiddleRight;
            bufer.TextAlign = ContentAlignment.MiddleLeft;
            //button.BackColor = Color.Khaki;
        }

        public abstract bool IsCompatibleWithBufer(IDataObject data);
    }
}
