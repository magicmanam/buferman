﻿using System;
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
        private readonly IProgramSettings _settings;
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
            IProgramSettings settings,
            IBufer bufer)
            : this(buferContextMenuGenerator, buferSelectionHandlerFactory, fileStorage, bufermanHost, settings, bufer)
        {

        }

        public BuferHandlersWrapper(
            IBuferContextMenuGenerator buferContextMenuGenerator,
            IBuferSelectionHandlerFactory buferSelectionHandlerFactory,
            IFileStorage fileStorage,
            IBufermanHost bufermanHost,
            IProgramSettings settings,
            IBufer bufer)
        {
            this._settings = settings;

            this._bufermanHost = bufermanHost;
            this._bufer = bufer;
            this._buferSelectionHandlerFactory = buferSelectionHandlerFactory;
            this._fileStorage = fileStorage;

            var buferTextRepresentation = this._bufer.ViewModel.Clip.GetData(DataFormats.UnicodeText) as string;
            if (string.IsNullOrEmpty(buferTextRepresentation))
            {
                buferTextRepresentation = this._bufer.ViewModel.Clip.GetData(DataFormats.StringFormat) as string;
                if (string.IsNullOrEmpty(buferTextRepresentation))
                {
                    buferTextRepresentation = this._bufer.ViewModel.Clip.GetData(DataFormats.Text) as string;
                }
            }

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
                    }
                }
                else
                {
                    buferText = this._MakeSpecialBuferText($"{buferText.Length}   {Resource.WhiteSpaces}");
                }
            }
            this._bufer.ViewModel.DefaultBackColor = this._bufer.BackColor;
            this._bufer.Text = buferText.Trim();
            this._bufer.ViewModel.OriginBuferText = this._bufer.Text;

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
            tooltipTitle = tooltipTitle ?? buferTitle;

            if (!string.IsNullOrWhiteSpace(tooltipTitle))
            {
                this._bufer.MouseOverTooltip.ToolTipTitle = tooltipTitle;
                this._bufer.FocusTooltip.ToolTipTitle = tooltipTitle;
            }

            if (formats.Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT))
            {
                this._bufer.ViewModel.Representation = this._bufer.ViewModel.Clip.GetData(ClipboardFormats.CUSTOM_IMAGE_FORMAT) as Image;
                this._bufer.MouseOverTooltip.IsBalloon = false;
                this._bufer.MouseOverTooltip.OwnerDraw = true;
                this._bufer.MouseOverTooltip.Popup += Tooltip_Popup;
                this._bufer.MouseOverTooltip.Draw += Tooltip_Draw;
            }

            this._bufer.AddOnFocusHandler(this._Bufer_GotFocus);
            this._bufer.AddOnUnfocusHandler(this._Bufer_LostFocus);

            var buferSelectionHandler = this._buferSelectionHandlerFactory.CreateHandler(this._bufer.ViewModel.Clip);
            this._bufer.AddOnClickHandler(buferSelectionHandler.DoOnClipSelection);

            bufer.SetContextMenu(buferContextMenuGenerator.GenerateContextMenuItems(this._bufer, isChangeTextAvailable, buferSelectionHandler, bufermanHost, buferTypeGenerator));
        }

        private string _MakeSpecialBuferText(string baseString)
        {
            return $"<< {baseString} >>";
        }

        private void Tooltip_Draw(object sender, DrawToolTipEventArgs e)
        {
            var preview = this._bufer.ViewModel.Representation as Image;

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
            var previewImage = this._bufer.ViewModel.Representation as Image;

            if (previewImage != null)
            {
                e.ToolTipSize = new Size((int)(previewImage.Width * IMAGE_SCALE), (int)(previewImage.Height * IMAGE_SCALE));
            }
        }

        private void _Bufer_GotFocus(object sender, EventArgs e)
        {
            var buferViewModel = this._bufer.ViewModel;

            this._bufer.BackColor = this._settings.FocusedBuferBackColor;

            if (buferViewModel != this._bufermanHost.LatestFocusedBufer)
            {
                this._bufermanHost.LatestFocusedBufer = buferViewModel;
                this._bufer.ShowFocusTooltip(buferViewModel.Representation as string, this._settings.BuferTooltipDuration);
            }
        }

        private void _Bufer_LostFocus(object sender, EventArgs e)
        {
            this._bufer.BackColor = this._bufer.ViewModel.DefaultBackColor;

            this._bufer.HideFocusTooltip();
        }
    }
}// TODO (m) relocate from this assembly