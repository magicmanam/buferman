using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Files;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.ContextMenu;

namespace BuferMAN.WinForms
{
    internal class BuferHandlersWrapper
    {
        private readonly IProgramSettingsGetter _settings;
        private readonly IBuferSelectionHandlerFactory _buferSelectionHandlerFactory;
        private readonly IFileStorage _fileStorage;
        private readonly IBufermanHost _bufermanHost;
        private readonly IBufer _bufer;
        private const float IMAGE_SCALE = 0.75f;

        public BuferHandlersWrapper(
            IDataObject dataObject,
            IBuferContextMenuGenerator buferContextMenuGenerator,
            IBuferSelectionHandlerFactory buferSelectionHandlerFactory,
            IFileStorage fileStorage,
            IBufermanHost bufermanHost,
            IProgramSettingsGetter settings,
            IBufer bufer)
            : this(buferContextMenuGenerator, buferSelectionHandlerFactory, fileStorage, bufermanHost, settings, bufer)
        {

        }

        public BuferHandlersWrapper(
            IBuferContextMenuGenerator buferContextMenuGenerator,
            IBuferSelectionHandlerFactory buferSelectionHandlerFactory,
            IFileStorage fileStorage,
            IBufermanHost bufermanHost,
            IProgramSettingsGetter settings,
            IBufer bufer)
        {
            this._settings = settings;

            this._bufermanHost = bufermanHost;
            this._bufer = bufer;
            this._buferSelectionHandlerFactory = buferSelectionHandlerFactory;
            this._fileStorage = fileStorage;

            var buferTextRepresentation = this._bufer.ViewModel.TextRepresentation;

            var formats = this._bufer.ViewModel.Clip.GetFormats();

            var isChangeTextAvailable = true;
            IBuferTypeMenuGenerator buferTypeGenerator = null;
            string buferTitle = null;
            string tooltipTitle = null;
            string buferText = null;
            if (buferTextRepresentation == null)
            {
                var files = this._bufer.ViewModel.Clip.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    isChangeTextAvailable = false;
                    var firstFile = files.First();
                    var onlyFolders = files.Select(f => this._fileStorage.GetFileAttributes(f).HasFlag(FileAttributes.Directory))
                        .All(f => f);

                    if (files.Length == 1)
                    {
                        buferText = onlyFolders ? Resource.FolderBufer : Resource.FileBufer;

                        const int MAX_FILE_LENGTH_FOR_BUFER_TITLE = 50;// TODO (m) into settings
                        if (firstFile.Length < MAX_FILE_LENGTH_FOR_BUFER_TITLE)
                        {
                            tooltipTitle = this._MakeSpecialBuferText(buferText);
                        }

                        buferTitle = this._MakeSpecialBuferText(firstFile.Length < MAX_FILE_LENGTH_FOR_BUFER_TITLE ? firstFile : buferText);

                        buferTypeGenerator = new FileBuferMenuGenerator(firstFile);
                    }
                    else
                    {
                        buferText = onlyFolders ? Resource.FoldersBufer : Resource.FilesBufer;
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
                        this._bufer.ApplyFontStyle(FontStyle.Italic | FontStyle.Bold);
                    }
                    else
                    {
                        if (formats.Contains(ClipboardFormats.FILE_CONTENTS_FORMAT))
                        {
                            isChangeTextAvailable = false;
                            buferTextRepresentation = this._MakeSpecialBuferText(Resource.FileContentsBufer);
                            this._bufer.ApplyFontStyle(FontStyle.Italic | FontStyle.Bold);
                        }
                    }
                }
            }

            buferText = buferTitle ?? buferTextRepresentation;
            if (string.IsNullOrWhiteSpace(buferText))
            {
                this._bufer.ApplyFontStyle(FontStyle.Italic | FontStyle.Bold);
                isChangeTextAvailable = false;

                if (buferText == null)
                {
                    if (formats.Any(f => string.Equals(f, ClipboardFormats.VISUAL_STUDIO_PROJECT_ITEMS, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        buferText = this._MakeSpecialBuferText("VISUAL STUDIO project items");
                    }
                    else
                    {
                        buferText = this._MakeSpecialBuferText(Resource.NotTextBufer);
                        // TODO (l) maybe track such cases and/or ask user to send info (at least formats list) of this bufer?
                        // Or user can think of some name for this combination of formats
                    }
                }
                else
                {
                    buferText = this._MakeSpecialBuferText($"{buferText.Length}   {Resource.WhiteSpaces}");
                    tooltipTitle = buferText;
                }
            }
            this._bufer.ViewModel.DefaultBackColor = this._bufer.BackColor;
            buferText = buferText.Trim();
            this._bufer.SetText(buferText);
            this._bufer.ViewModel.OriginBuferTitle = buferText;

            int maxBuferLength = this._settings.MaxBuferPresentationLength;
            if (isChangeTextAvailable && buferTextRepresentation != null && buferTextRepresentation.Length > maxBuferLength)
            {
                buferTextRepresentation = buferTextRepresentation.Substring(0, maxBuferLength - 300) + Environment.NewLine + Environment.NewLine + "...";

                if (string.IsNullOrEmpty(buferTitle))
                {
                    buferTitle = this._MakeSpecialBuferText(Resource.BigTextBufer);
                }
            }

            this._bufer.ViewModel.Representation = buferTextRepresentation;// Maybe store original presentation as well ?
            this._bufer.SetMouseOverToolTip(buferTextRepresentation);// TODO (s) an issue here: on alias change this tooltip will show wront tooltip
            this._bufer.ViewModel.TextRepresentation = buferTextRepresentation;
            tooltipTitle = tooltipTitle ?? buferTitle;

            if (!string.IsNullOrWhiteSpace(tooltipTitle))
            {
                this._bufer.MouseOverTooltip.ToolTipTitle = tooltipTitle;
                this._bufer.FocusTooltip.ToolTipTitle = tooltipTitle;
            }

            if (formats.Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT))
            {
                var image = this._bufer.ViewModel.Clip.GetData(ClipboardFormats.CUSTOM_IMAGE_FORMAT) as Image;
                if (image != null)
                {
                    this._bufer.ViewModel.Representation = image;
                    this._bufer.MouseOverTooltip.IsBalloon = false;
                    this._bufer.MouseOverTooltip.OwnerDraw = true;
                    this._bufer.MouseOverTooltip.Popup += (object sender, PopupEventArgs e) =>
                    {
                        e.ToolTipSize = new Size((int)(image.Width * IMAGE_SCALE), (int)(image.Height * IMAGE_SCALE));
                    };
                    this._bufer.MouseOverTooltip.Draw += (object sender, DrawToolTipEventArgs e) =>
                    {
                        using (var b = new TextureBrush(new Bitmap(image)))
                        {
                            b.ScaleTransform(IMAGE_SCALE, IMAGE_SCALE);

                            var g = e.Graphics;
                            g.FillRectangle(b, e.Bounds);
                        }
                    };
                }
            }

            this._bufer.AddOnFocusHandler(this._Bufer_GotFocus);
            this._bufer.AddOnUnfocusHandler(this._Bufer_LostFocus);

            var buferSelectionHandler = this._buferSelectionHandlerFactory.CreateHandler(this._bufer.ViewModel.Clip, bufermanHost);
            this._bufer.AddOnClickHandler(buferSelectionHandler.DoOnClipSelection);

            bufer.SetContextMenu(buferContextMenuGenerator.GenerateContextMenuItems(this._bufer, isChangeTextAvailable, buferSelectionHandler, bufermanHost, buferTypeGenerator));
        }

        private string _MakeSpecialBuferText(string baseString)
        {
            return $"<< {baseString} >>";
        }

        private void _Bufer_GotFocus(object sender, EventArgs e)
        {
            var buferViewModel = this._bufer.ViewModel;

            this._bufer.BackColor = this._settings.FocusedBuferBackgroundColor;

            if (buferViewModel != this._bufermanHost.LatestFocusedBufer)
            {
                this._bufermanHost.LatestFocusedBufer = buferViewModel;

                if (this._settings.ShowFocusTooltip)
                {
                    this._bufer.ShowFocusTooltip(buferViewModel.TextRepresentation, this._settings.FocusTooltipDuration);
                }
            }
        }

        private void _Bufer_LostFocus(object sender, EventArgs e)
        {
            this._bufer.BackColor = this._bufer.ViewModel.DefaultBackColor;

            if (this._settings.ShowFocusTooltip)
            {
                this._bufer.HideFocusTooltip();
            }
        }
    }
}// TODO (m) relocate from this assembly