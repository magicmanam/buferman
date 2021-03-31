using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Clipboard;
using BuferMAN.Form.Properties;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.View;

namespace BuferMAN.Form
{
    public class BuferHandlersWrapper
    {
        private const int TOOLTIP_DURATION = 2500;
        private readonly BuferViewModel _buferViewModel;
        private readonly IBuferSelectionHandlerFactory _buferSelectionHandlerFactory;
        private readonly IFileStorage _fileStorage;
        private readonly Button _button;
        private readonly ToolTip _focusTooltip = new ToolTip() { OwnerDraw = false };
        private const float IMAGE_SCALE = 0.75f;

        public BuferHandlersWrapper(IDataObject dataObject, Button button, IBuferContextMenuGenerator buferContextMenuGenerator, IBuferSelectionHandlerFactory buferSelectionHandlerFactory, IFileStorage fileStorage, IBuferMANHost buferMANHost, IBufer bufer = null)
            : this(new BuferViewModel { Clip = dataObject, CreatedAt = DateTime.Now }, button, buferContextMenuGenerator, buferSelectionHandlerFactory, fileStorage, buferMANHost, bufer)
        {

        }

        public BuferHandlersWrapper(BuferViewModel buferViewModel, Button button, IBuferContextMenuGenerator buferContextMenuGenerator, IBuferSelectionHandlerFactory buferSelectionHandlerFactory, IFileStorage fileStorage, IBuferMANHost buferMANHost, IBufer bufer = null)
        {
            // TODO : remove Button button parameter
            this._buferViewModel = buferViewModel;
            this._button = button;
            this._buferSelectionHandlerFactory = buferSelectionHandlerFactory;
            this._fileStorage = fileStorage;

            var buferTextRepresentation = buferViewModel.Clip.GetData(DataFormats.UnicodeText) as string;
            if (string.IsNullOrEmpty(buferTextRepresentation))
            {
                buferTextRepresentation = buferViewModel.Clip.GetData(DataFormats.StringFormat) as string;
                if (string.IsNullOrEmpty(buferTextRepresentation))
                {
                    buferTextRepresentation = buferViewModel.Clip.GetData(DataFormats.Text) as string;
                }
            }

            var formats = buferViewModel.Clip.GetFormats();

            var isChangeTextAvailable = true;
            string buferTitle = null;
            string tooltipTitle = null;
            if (buferTextRepresentation == null)
            {
                var files = buferViewModel.Clip.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    isChangeTextAvailable = false;
                    var firstFile = files.First();
                    var onlyFolders = files.Select(f => this._fileStorage.GetFileAttributes(f).HasFlag(FileAttributes.Directory))
                        .All(f => f);

                    if (files.Length == 1)
                    {
                        var buferText = onlyFolders ? Resource.FolderBufer : Resource.FileBufer;

                        const int MAX_FILE_LENGTH_FOR_BUFER_TITLE = 50;
                        if (firstFile.Length < MAX_FILE_LENGTH_FOR_BUFER_TITLE)
                        {
                            tooltipTitle = this._MakeSpecialBuferText(buferText);
                        }

                        buferTitle = this._MakeSpecialBuferText(firstFile.Length < MAX_FILE_LENGTH_FOR_BUFER_TITLE ? firstFile : buferText);
                    }
                    else
                    {
                        var buferText = onlyFolders ? Resource.FoldersBufer : Resource.FilesBufer;
                        buferTitle = this._MakeSpecialBuferText($"{buferText} ({files.Length})");
                    }

                    var folder = this._fileStorage.GetFileDirectory(firstFile);
                    buferTextRepresentation += folder + Environment.NewLine + Environment.NewLine;
                    buferTextRepresentation += string.Join(Environment.NewLine, files.Select(f => this._fileStorage.GetFileName(f) + (this._fileStorage.GetFileAttributes(f).HasFlag(FileAttributes.Directory) ? Path.DirectorySeparatorChar.ToString() : string.Empty)).ToList());
                }
                else
                {
                    var isBitmap = formats.Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT);
                    if (isBitmap)
                    {
                        isChangeTextAvailable = false;
                        buferTextRepresentation = this._MakeSpecialBuferText(Resource.ImageBufer);
                        this._MakeItalicBoldFont(button);
                    }
                    else
                    {
                        if (formats.Contains(ClipboardFormats.FILE_CONTENTS_FORMAT))
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
                if (buttonText == null)
                {// Can I delete this bufer? Check that "clip.GetFormats() == 0"
                    // Maybe in debug mode I will catch this case and will find more details how it can happens
                }
                this._MakeItalicBoldFont(button);
                isChangeTextAvailable = false;
            }
            buferViewModel.DefaultBackColor = button.BackColor;
            buferViewModel.Representation = buferTextRepresentation;
            button.Tag = buferViewModel;
            button.Text = buttonText.Trim();
            buferViewModel.OriginBuferText = button.Text;

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
            tooltipTitle = tooltipTitle ?? buferTitle;

            if (!string.IsNullOrWhiteSpace(tooltipTitle))
            {
                tooltip.ToolTipTitle = tooltipTitle;
                this._focusTooltip.ToolTipTitle = tooltipTitle;
            }
            
            if (formats.Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT))
            {
                buferViewModel.Representation = this._buferViewModel.Clip.GetData(ClipboardFormats.CUSTOM_IMAGE_FORMAT) as Image;
                tooltip.IsBalloon = false;
                tooltip.OwnerDraw = true;
                tooltip.Popup += Tooltip_Popup;
                tooltip.Draw += Tooltip_Draw;
            }

            button.GotFocus += Bufer_GotFocus;
            button.LostFocus += Bufer_LostFocus;

            var buferSelectionHandler = this._buferSelectionHandlerFactory.CreateHandler(this._buferViewModel.Clip);
            button.Click += buferSelectionHandler.DoOnClipSelection;

            //bufer.SetContextMenu(clipMenuGenerator.GenerateContextMenu(this._buferViewModel, button, tooltip, isChangeTextAvailable, buferSelectionHandler));
            button.ContextMenu = buferContextMenuGenerator.GenerateContextMenu(this._buferViewModel, button, tooltip, isChangeTextAvailable, buferSelectionHandler, buferMANHost);
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
            var preview = (parent.Tag as BuferViewModel).Representation as Image;

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
            var previewImage = (parent.Tag as BuferViewModel).Representation as Image;

            if (previewImage != null)
            {
                e.ToolTipSize = new Size((int) (previewImage.Width * IMAGE_SCALE), (int)(previewImage.Height * IMAGE_SCALE));
            }
        }

        private void Bufer_GotFocus(object sender, EventArgs e)
        {
            this._focusTooltip.Show((this._button.Tag as BuferViewModel).Representation as string, this._button, TOOLTIP_DURATION);
        }

        private void Bufer_LostFocus(object sender, EventArgs e)
        {
            this._focusTooltip.Hide(this._button);
        }
    }
}