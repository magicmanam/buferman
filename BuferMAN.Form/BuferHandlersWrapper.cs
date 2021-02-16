using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Clipboard;
using BuferMAN.Form.Properties;
using SystemWindowsForm = System.Windows.Forms.Form;
using BuferMAN.Infrastructure;
using BuferMAN.ContextMenu;
using BuferMAN.Infrastructure.Storage;

namespace BuferMAN.Form
{
    public class BuferHandlersWrapper
    {
        private const int TOOLTIP_DURATION = 2500;
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IDataObject _dataObject;
        private readonly IBuferSelectionHandler _buferSelectionHandler;
        private readonly IFileStorage _fileStorage;
        private readonly Button _button;
        private readonly ToolTip _focusTooltip = new ToolTip() { OwnerDraw = false };
        private const float IMAGE_SCALE = 0.75f;

        public BuferHandlersWrapper(IClipboardBuferService clipboardBuferService, IDataObject dataObject, Button button, SystemWindowsForm form, IClipMenuGenerator clipMenuGenerator, IBuferSelectionHandler buferSelectionHandler, IFileStorage fileStorage)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._dataObject = dataObject;
            this._button = button;
            this._buferSelectionHandler = buferSelectionHandler;
            this._fileStorage = fileStorage;

            var buferTextRepresentation = dataObject.GetData(DataFormats.UnicodeText) as string;
            if (string.IsNullOrEmpty(buferTextRepresentation))
            {
                buferTextRepresentation = dataObject.GetData(DataFormats.StringFormat) as string;
                if (string.IsNullOrEmpty(buferTextRepresentation))
                {
                    buferTextRepresentation = dataObject.GetData(DataFormats.Text) as string;
                }
            }
            
            var isChangeTextAvailable = true;
            string buferTitle = null;
            if (buferTextRepresentation == null)
            {
                var files = dataObject.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    isChangeTextAvailable = false;

                    if (files.Length == 1)
                    {
                        buferTitle = this._MakeSpecialBuferText(Resource.FileBufer);
                    }
                    else
                    {
                        buferTitle = this._MakeSpecialBuferText($"{Resource.FilesBufer} ({files.Length})");
                    }

                    var folder = this._fileStorage.GetFileDirectory(files.First());
                    buferTextRepresentation += folder + Environment.NewLine + Environment.NewLine;
                    buferTextRepresentation += string.Join(Environment.NewLine, files.Select(f => this._fileStorage.GetFileName(f) + (this._fileStorage.GetFileAttributes(f).HasFlag(FileAttributes.Directory) ? Path.DirectorySeparatorChar.ToString() : string.Empty)).ToList());
                }
                else
                {
                    var isBitmap = dataObject.GetFormats().Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT);
                    if (isBitmap)
                    {
                        isChangeTextAvailable = false;
                        buferTextRepresentation = this._MakeSpecialBuferText(Resource.ImageBufer);
                        this._MakeItalicBoldFont(button);
                    }
                    else
                    {
                        if (dataObject.GetFormats().Contains(ClipboardFormats.FILE_CONTENTS_FORMAT))
                        {
                            isChangeTextAvailable = false;
                            buferTextRepresentation = this._MakeSpecialBuferText(Resource.FileContentsBufer);
                            this._MakeItalicBoldFont(button);
                        }
                    }
                }
            }
            
            string buttonText = buferTitle ?? buferTextRepresentation;
            if (string.IsNullOrWhiteSpace(buttonText))
            {
                buttonText = this._MakeSpecialBuferText(buttonText == null ? Resource.NotTextBufer : $"{buttonText.Length}   {Resource.WhiteSpaces}");
                this._MakeItalicBoldFont(button);
                isChangeTextAvailable = false;
            }

            var buttonData = new ButtonData
            {
                Representation = buferTextRepresentation,
                DefaultBackColor = button.BackColor
            };
            button.Tag = buttonData;
            button.Text = buttonText.Trim();

            string originBuferText = button.Text;

            int maxBuferLength = 2000;// Into settings (not more then 5000 - add validation)
            if (buferTextRepresentation != null && buferTextRepresentation.Length > maxBuferLength + 300)
            {
                buferTextRepresentation = buferTextRepresentation.Substring(0, maxBuferLength) + Environment.NewLine + Environment.NewLine + "...";

                if (string.IsNullOrEmpty(buferTitle))
                {
                    buferTitle = this._MakeSpecialBuferText(Resource.BigTextBufer);
                }
            }

            var tooltip = new ToolTip() { InitialDelay = 0 };
            tooltip.IsBalloon = true;
            tooltip.SetToolTip(button, buferTextRepresentation);
            if (!string.IsNullOrWhiteSpace(buferTitle))
            {
                tooltip.ToolTipTitle = buferTitle;
                this._focusTooltip.ToolTipTitle = buferTitle;
            }

            if (this._dataObject.GetFormats().Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT))
            {
                buttonData.Representation = this._dataObject.GetData(ClipboardFormats.CUSTOM_IMAGE_FORMAT) as Image;
                tooltip.IsBalloon = false;
                tooltip.OwnerDraw = true;
                tooltip.Popup += Tooltip_Popup;
                tooltip.Draw += Tooltip_Draw;
            }

            button.GotFocus += Bufer_GotFocus;
            button.LostFocus += Bufer_LostFocus;

            button.Click += this._buferSelectionHandler.DoOnClipSelection;

            button.ContextMenu = clipMenuGenerator.GenerateContextMenu(this._dataObject, button, tooltip, isChangeTextAvailable);
        }

        private void _MakeItalicBoldFont(Button button)
        {
            button.Font = new Font(button.Font, FontStyle.Italic | FontStyle.Bold);
        }

        private string _MakeSpecialBuferText(string baseString)
        {
            return $"<< {baseString} >>";
        }

        private void Tooltip_Draw(object sender, DrawToolTipEventArgs e)
        {
            // to set the tag for each button or object
            Control parent = e.AssociatedControl;
            var preview = (parent.Tag as ButtonData).Representation as Image;

            if (preview != null)
            {
                Graphics g = e.Graphics;

                //create your own custom brush to fill the background with the image
                TextureBrush b = new TextureBrush(new Bitmap(preview));
                b.ScaleTransform(IMAGE_SCALE, IMAGE_SCALE);

                g.FillRectangle(b, e.Bounds);
                b.Dispose();
            }
        }

        private void Tooltip_Popup(object sender, PopupEventArgs e)
        {
            Control parent = e.AssociatedControl;
            var previewImage = (parent.Tag as ButtonData).Representation as Image;

            if (previewImage != null)
            {
                e.ToolTipSize = new Size((int) (previewImage.Width * IMAGE_SCALE), (int)(previewImage.Height * IMAGE_SCALE));
            }
        }

        private void Bufer_GotFocus(object sender, EventArgs e)
        {
            this._focusTooltip.Show((this._button.Tag as ButtonData).Representation as string, this._button, TOOLTIP_DURATION);
        }

        private void Bufer_LostFocus(object sender, EventArgs e)
        {
            this._focusTooltip.Hide(this._button);
        }
    }
}