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
using System.Collections.Generic;
using BuferMAN.Infrastructure.Plugins;

namespace BuferMAN.WinForms
{
    internal class BuferHandlersWrapper
    {
        private readonly IProgramSettingsGetter _settingsGetter;
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IProgramSettingsSetter _settingsSetter;
        private readonly IBuferSelectionHandlerFactory _buferSelectionHandlerFactory;
        private readonly IFileStorage _fileStorage;
        private readonly IBufermanHost _bufermanHost;
        private readonly IBufer _bufer;
        private const float IMAGE_SCALE = 0.75f;

        public BuferHandlersWrapper(
            IClipboardBuferService clipboardBuferService,
            IBuferContextMenuGenerator buferContextMenuGenerator,
            IBuferSelectionHandlerFactory buferSelectionHandlerFactory,
            IFileStorage fileStorage,
            IBufermanHost bufermanHost,
            IProgramSettingsGetter settingsGetter,
            IProgramSettingsSetter settingsSetter,
            IEnumerable<IBufermanPlugin> plugins,
            IBufer bufer)
        {
            this._settingsGetter = settingsGetter;
            this._settingsSetter = settingsSetter;

            this._clipboardBuferService = clipboardBuferService;
            this._bufermanHost = bufermanHost;
            this._bufer = bufer;
            this._buferSelectionHandlerFactory = buferSelectionHandlerFactory;
            this._fileStorage = fileStorage;

            var buferTextRepresentation = this._bufer.ViewModel.TextRepresentation;

            var formats = this._bufer.ViewModel.Clip.GetFormats();

            string buferTitle = null;

            if (buferTextRepresentation == null)
            {
                var clipFiles = this._bufer.ViewModel.Clip.GetData(DataFormats.FileDrop) as string[];
                if (clipFiles != null && clipFiles.Length > 0)
                {
                    this._bufer.ViewModel.IsChangeTextAvailable = false;
                    var firstFile = clipFiles.First();
                    var onlyFolders = clipFiles.Select(f => this._fileStorage.GetFileAttributes(f).HasFlag(FileAttributes.Directory))
                        .All(f => f);

                    if (clipFiles.Length == 1)
                    {
                        const int MAX_FILE_LENGTH_FOR_BUFER_TITLE = 50;// TODO (m) into settings
                        buferTitle = this._MakeSpecialBuferText(
                            firstFile.Length < MAX_FILE_LENGTH_FOR_BUFER_TITLE ?
                            firstFile :
                            (onlyFolders ? Resource.FolderBufer : Resource.FileBufer));// TODO (m) these resources are duplicated in BuferMAN.Application project
                    }
                    else
                    {
                        buferTitle = this._MakeSpecialBuferText($"{(onlyFolders ? Resource.FoldersBufer : Resource.FilesBufer)} ({clipFiles.Length})");
                    }

                    var folder = this._fileStorage.GetFileDirectory(firstFile);
                    buferTextRepresentation += folder + Environment.NewLine + Environment.NewLine;
                    buferTextRepresentation += string.Join(Environment.NewLine, clipFiles.Select(f => this._fileStorage.GetFileName(f) + (this._fileStorage.GetFileAttributes(f).HasFlag(FileAttributes.Directory) ? Path.DirectorySeparatorChar.ToString() : string.Empty)).ToList());
                }
                else
                {
                    var isBitmap = formats.Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT);
                    if (isBitmap)
                    {
                        this._bufer.ViewModel.IsChangeTextAvailable = false;
                        buferTextRepresentation = this._MakeSpecialBuferText(Resource.ImageBufer);
                        this._bufer.ApplyFontStyle(FontStyle.Italic | FontStyle.Bold);
                    }
                    else
                    {
                        if (formats.Contains(ClipboardFormats.FILE_CONTENTS_FORMAT))
                        {
                            this._bufer.ViewModel.IsChangeTextAvailable = false;
                            buferTextRepresentation = this._MakeSpecialBuferText(Resource.FileContentsBufer);
                            this._bufer.ApplyFontStyle(FontStyle.Italic | FontStyle.Bold);
                        }
                    }
                }
            }

            string buferText = buferTitle ?? buferTextRepresentation;
            if (string.IsNullOrWhiteSpace(buferText))
            {
                this._bufer.ApplyFontStyle(FontStyle.Italic | FontStyle.Bold);
                this._bufer.ViewModel.IsChangeTextAvailable = false;

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
                    this._bufer.ViewModel.TooltipTitle = buferText;
                }
            }
            this._bufer.ViewModel.DefaultBackColor = this._bufer.BackColor;
            buferText = buferText.Trim();
            this._bufer.SetText(buferText);
            this._bufer.ViewModel.OriginBuferTitle = buferText;

            this._bufer.ViewModel.Representation = buferTextRepresentation;// TODO (m) Maybe store original presentation as well ?
            this._bufer.SetMouseOverToolTip(buferTextRepresentation);
            this._bufer.ViewModel.TextRepresentation = buferTextRepresentation;
            this._bufer.ViewModel.TooltipTitle = this._bufer.ViewModel.TooltipTitle ?? buferTitle;

            if (!string.IsNullOrWhiteSpace(this._bufer.ViewModel.TooltipTitle))
            {
                this._bufer.MouseOverTooltip.ToolTipTitle = this._bufer.ViewModel.TooltipTitle;
                this._bufer.FocusTooltip.ToolTipTitle = this._bufer.ViewModel.TooltipTitle;
            }

            IBuferTypeMenuGenerator buferTypeMenuGenerator = null;
            if (Uri.TryCreate(this._bufer.ViewModel.OriginBuferTitle, UriKind.Absolute, out var uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                buferTypeMenuGenerator = new HttpUrlBuferMenuGenerator(this._bufer.ViewModel.OriginBuferTitle, this._settingsGetter, this._settingsSetter);
            }
            else
            {
                var files = this._bufer.ViewModel.Clip.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    var firstFile = files.First();

                    if (files.Length == 1)
                    {
                        buferTypeMenuGenerator = new FileBuferMenuGenerator(firstFile);
                    }
                }
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
            if (this._settingsGetter.IsBuferClickingExplained)
            {
                this._bufer.AddOnClickHandler(buferSelectionHandler.DoOnClipSelection);
            }
            else
            {
                this._bufer.AddOnClickHandler((object sender, EventArgs e) =>
                {
                    if (this._settingsGetter.IsBuferClickingExplained)
                    {
                        buferSelectionHandler.DoOnClipSelection(sender, e);
                    }
                    else
                    {
                        this._bufermanHost.UserInteraction.ShowPopup(Resource.BuferClickingExplanationText, Application.ProductName);
                        this._settingsSetter.MarkThatBuferClickingWasExplained();
                    }
                });
            }

            var buferContextMenuState = new BuferContextMenuState(
                clipboardBuferService,
                buferSelectionHandler,
                bufermanHost,
                () => Resource.MenuPin, // TODO (m) remove these actions
                () => Resource.MenuUnpin,
                () => Resource.MenuAddedToFile,
                bufer);

            bufer.SetContextMenu(buferContextMenuGenerator.GenerateContextMenuItems(buferContextMenuState, bufermanHost, buferTypeMenuGenerator));

            foreach (var plugin in plugins) if (plugin.Available && plugin.Enabled)
                {
                    plugin.UpdateBuferItem(buferContextMenuState);
                }
        }

        private string _MakeSpecialBuferText(string baseString)
        {
            return $"<< {baseString} >>";
        }// TODO (m) is duplicated in DataObjectHandler and BigTextBuferPlugin

        private void _Bufer_GotFocus(object sender, EventArgs e)
        {
            var buferViewModel = this._bufer.ViewModel;

            this._bufer.BackColor = this._settingsGetter.FocusedBuferBackgroundColor;

            if (buferViewModel != this._bufermanHost.LatestFocusedBufer)
            {
                this._bufermanHost.LatestFocusedBufer = buferViewModel;

                if (this._settingsGetter.ShowFocusTooltip)
                {
                    this._bufer.ShowFocusTooltip(buferViewModel.TextRepresentation, this._settingsGetter.FocusTooltipDuration);
                }
            }
        }

        private void _Bufer_LostFocus(object sender, EventArgs e)
        {
            this._bufer.BackColor = this._bufer.ViewModel.DefaultBackColor;

            if (this._settingsGetter.ShowFocusTooltip)
            {
                this._bufer.HideFocusTooltip();
            }
        }
    }
}// TODO (m) relocate from this assembly