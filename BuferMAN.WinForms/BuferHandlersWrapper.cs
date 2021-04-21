using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Files;
using BuferMAN.View;
using BuferMAN.Infrastructure.Settings;

namespace BuferMAN.WinForms
{
    internal class BuferHandlersWrapper
    {
        private readonly IProgramSettings _settings;
        private readonly BuferViewModel _buferViewModel;
        private readonly IBuferSelectionHandlerFactory _buferSelectionHandlerFactory;
        private readonly IFileStorage _fileStorage;
        private readonly IBufermanHost _bufermanHost;
        private readonly Button _button;
        private readonly ToolTip _focusTooltip = new ToolTip() { OwnerDraw = false };
        private const float IMAGE_SCALE = 0.75f;

        public BuferHandlersWrapper(
            IDataObject dataObject,
            Button button,
            IBuferContextMenuGenerator buferContextMenuGenerator,
            IBuferSelectionHandlerFactory buferSelectionHandlerFactory,
            IFileStorage fileStorage,
            IBufermanHost bufermanHost, 
            IProgramSettings settings, 
            IBufer bufer)
            : this(new BuferViewModel { Clip = dataObject, CreatedAt = DateTime.Now }, button, buferContextMenuGenerator, buferSelectionHandlerFactory, fileStorage, bufermanHost, settings, bufer)
        {

        }

        public BuferHandlersWrapper(
            BuferViewModel buferViewModel,
            Button button,
            IBuferContextMenuGenerator buferContextMenuGenerator,
            IBuferSelectionHandlerFactory buferSelectionHandlerFactory, 
            IFileStorage fileStorage, 
            IBufermanHost bufermanHost, 
            IProgramSettings settings, 
            IBufer bufer)
        {
            this._settings = settings;

            // TODO (l) : remove Button button parameter
            this._bufermanHost = bufermanHost;
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
                        this._button.Font = this._MakeItalicBoldFont(this._button.Font);
                    }
                    else
                    {
                        if (formats.Contains(ClipboardFormats.FILE_CONTENTS_FORMAT))
                        {
                            isChangeTextAvailable = false;
                            buferTextRepresentation = this._MakeSpecialBuferText(Resource.FileContentsBufer);
                            this._button.Font = this._MakeItalicBoldFont(this._button.Font);
                        }
                    }
                }
            }
            
            string buttonText = buferTitle ?? buferTextRepresentation;
            if (string.IsNullOrWhiteSpace(buttonText))
            {
                if (buttonText == null)
                {
                    // TODO (m) I need more info about such situation. Maybe log some information. VS project items copied - this situation
                }
                buttonText = this._MakeSpecialBuferText(buttonText == null ? Resource.NotTextBufer : $"{buttonText.Length}   {Resource.WhiteSpaces}");
                this._button.Font = this._MakeItalicBoldFont(this._button.Font);
                isChangeTextAvailable = false;
            }
            buferViewModel.DefaultBackColor = this._button.BackColor;
            this._button.Tag = buferViewModel;
            this._button.Text = buttonText.Trim();
            buferViewModel.OriginBuferText = this._button.Text;

            int maxBuferLength = this._settings.MaxBuferPresentationLength;
            if (isChangeTextAvailable && buferTextRepresentation != null && buferTextRepresentation.Length > maxBuferLength)
            {
                buferTextRepresentation = buferTextRepresentation.Substring(0, maxBuferLength - 300) + Environment.NewLine + Environment.NewLine + "...";

                if (string.IsNullOrEmpty(buferTitle))
                {
                    buferTitle = this._MakeSpecialBuferText(Resource.BigTextBufer);
                }
            }

            buferViewModel.Representation = buferTextRepresentation;// Maybe store original presentation as well ?
            var tooltip = new ToolTip() { InitialDelay = 0 };
            tooltip.IsBalloon = true;
            tooltip.SetToolTip(this._button, buferTextRepresentation);// TODO (s) an issue here: on alias change this tooltip will show wront tooltip
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

            this._button.GotFocus += Bufer_GotFocus;
            this._button.LostFocus += Bufer_LostFocus;

            var buferSelectionHandler = this._buferSelectionHandlerFactory.CreateHandler(this._buferViewModel.Clip);
            this._button.Click += buferSelectionHandler.DoOnClipSelection;

            bufer.SetButton(this._button);
            bufer.SetContextMenu(buferContextMenuGenerator.GenerateContextMenu(this._buferViewModel, this._button, tooltip, isChangeTextAvailable, buferSelectionHandler, bufermanHost));
        }

        private Font _MakeItalicBoldFont(Font font)
        {
            return new Font(font, FontStyle.Italic | FontStyle.Bold);
        }

        private string _MakeSpecialBuferText(string baseString)
        {
            return $"<< {baseString} >>";
        }

        private void Tooltip_Draw(object sender, DrawToolTipEventArgs e)
        {
            var preview = this._buferViewModel.Representation as Image;

            if (preview != null)
            {
                using (var b = new TextureBrush(new Bitmap(preview)))
                {
                    b.ScaleTransform(IMAGE_SCALE, IMAGE_SCALE);

                    var g = e.Graphics;
                    g.FillRectangle(b, e.Bounds);
                }
            }
        }

        private void Tooltip_Popup(object sender, PopupEventArgs e)
        {
            var previewImage = this._buferViewModel.Representation as Image;

            if (previewImage != null)
            {
                e.ToolTipSize = new Size((int) (previewImage.Width * IMAGE_SCALE), (int)(previewImage.Height * IMAGE_SCALE));
            }
        }

        private void Bufer_GotFocus(object sender, EventArgs e)
        {
            var buferViewModel = this._buferViewModel;

            if (buferViewModel != this._bufermanHost.LatestFocusedBufer)
            {
                this._bufermanHost.LatestFocusedBufer = buferViewModel;
                this._focusTooltip.Show(buferViewModel.Representation as string, this._button, this._settings.BuferTooltipDuration);
            }
        }

        private void Bufer_LostFocus(object sender, EventArgs e)
        {
            this._focusTooltip.Hide(this._button);
        }
    }
}