using BuferMAN.View;
using System.Linq;
using BuferMAN.Infrastructure;
using BuferMAN.Clipboard;
using magicmanam.UndoRedo;
using System;
using BuferMAN.Infrastructure.Settings;
using System.Windows.Forms;
using System.IO;
using BuferMAN.Infrastructure.Files;

namespace BuferMAN.Application
{
    internal class DataObjectHandler : IIDataObjectHandler
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IProgramSettingsGetter _settings;
        private readonly IFileStorage _fileStorage;

        public event EventHandler<ClipboardUpdatedEventArgs> Updated;
        public event EventHandler Full;

        public DataObjectHandler(IClipboardBuferService clipboardBuferService, IProgramSettingsGetter settings, IFileStorage fileStorage)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._settings = settings;
            this._fileStorage = fileStorage;
        }

        // For unit tests:
        // not persistent: last temp ? <nothing> :
        // any temp ? <swap> : 
        // pinned ? <nothing> :
        // (not exist) - <add temp>

        // persistent: pinned ? <nothing> :
        // any temp ? <remove from temp and add pinned> : 
        // (not exist) <add pinned>
        public bool TryHandleDataObject(BuferViewModel buferViewModel)
        {
            buferViewModel.IsChangeTextAvailable = true;

            buferViewModel.TextData = this._GetNotEmptyStringData(buferViewModel.Clip, DataFormats.UnicodeText, DataFormats.StringFormat, DataFormats.Text);
            buferViewModel.TextRepresentation = buferViewModel.TextData;

            if (buferViewModel.Clip.GetData(DataFormats.StringFormat) as string == string.Empty)
            {
                // TODO (m) maybe set System.String data ? Can be implemented via setting. Such bufers can be marked and maybe suggest to user paste it as a usual text (Ctrl + A)
            }

            var files = buferViewModel.Clip.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0)
            {
                buferViewModel.IsChangeTextAvailable = false;
                var firstFile = files.First();
                var onlyFolders = files.Select(f => this._fileStorage.GetFileAttributes(f).HasFlag(FileAttributes.Directory))
                    .All(f => f);

                if (files.Length == 1)
                {
                    const int MAX_FILE_LENGTH_FOR_BUFER_TITLE = 50;// TODO (m) into settings
                    if (firstFile.Length < MAX_FILE_LENGTH_FOR_BUFER_TITLE)
                    {
                        buferViewModel.TooltipTitle = this._MakeSpecialBuferText(onlyFolders ? Resource.FolderBufer : Resource.FileBufer);
                    }
                }

                var folder = this._fileStorage.GetFileDirectory(firstFile);
                buferViewModel.TextRepresentation += folder + Environment.NewLine + Environment.NewLine;
                buferViewModel.TextRepresentation += string.Join(Environment.NewLine, files.Select(f => this._fileStorage.GetFileName(f) + (this._fileStorage.GetFileAttributes(f).HasFlag(FileAttributes.Directory) ? Path.DirectorySeparatorChar.ToString() : string.Empty)).ToList());
            }
            else
            {
                if (buferViewModel.Clip.GetFormats().Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT))
                {
                    buferViewModel.IsChangeTextAvailable = false;
                    buferViewModel.TextRepresentation = this._MakeSpecialBuferText(Resource.ImageBufer);
                }
                else
                {
                    if (buferViewModel.Clip.GetFormats().Contains(ClipboardFormats.FILE_CONTENTS_FORMAT))
                    {
                        buferViewModel.IsChangeTextAvailable = false;
                        buferViewModel.TextRepresentation = this._MakeSpecialBuferText(Resource.FileContentsBufer);
                    }
                }
            }

            buferViewModel.Representation = buferViewModel.TextRepresentation;// TODO (m) Maybe store original presentation as well ?

            var isLastTempBufer = this._clipboardBuferService.IsLastTemporaryBufer(buferViewModel);

            if (!buferViewModel.Pinned && isLastTempBufer) // Repeated Ctrl + C operation
            {
                buferViewModel.ViewId = this._clipboardBuferService.LastTemporaryBufer.ViewId;
                this.Updated?.Invoke(this, new ClipboardUpdatedEventArgs(buferViewModel));
                return false;
            }

            if (this._clipboardBuferService.IsInPinnedBufers(buferViewModel, out Guid pinnedBuferViewId))
            {
                buferViewModel.ViewId = pinnedBuferViewId;
                this.Updated?.Invoke(this, new ClipboardUpdatedEventArgs(buferViewModel));
                return false;
            }

            var alreadyInTempBufers = this._clipboardBuferService.IsInTemporaryBufers(buferViewModel, out Guid viewId);

            if (!alreadyInTempBufers && this._clipboardBuferService.GetPinnedBufers().Count() == this._settings.MaxBufersCount)
            {   // Maybe we should not do any check if persistent clips count = max bufers count
                // Maybe all visible bufers can not be persistent (create a limit of persistent bufers)?
                this.Full?.Invoke(this, EventArgs.Empty);
                this.Updated?.Invoke(this, new ClipboardUpdatedEventArgs(buferViewModel));
                return false;
            }

            using (UndoableContext<ApplicationStateSnapshot>.Current.StartAction())
            {
                if (alreadyInTempBufers)
                {
                    this._clipboardBuferService.RemoveBufer(viewId);
                }
                else if (this._clipboardBuferService.BufersCount == this._settings.MaxBufersCount + this._settings.ExtraBufersCount)
                {
                    this._clipboardBuferService.RemoveBufer(this._clipboardBuferService.FirstTemporaryBufer.ViewId);
                }

                buferViewModel.ViewId = Guid.NewGuid();
                this._clipboardBuferService.AddTemporaryClip(buferViewModel);

                if (buferViewModel.Pinned)
                {
                    this._clipboardBuferService.TryPinBufer(buferViewModel.ViewId);
                }
            }

            this.Updated?.Invoke(this, new ClipboardUpdatedEventArgs(buferViewModel));
            return true;
        }

        private string _GetNotEmptyStringData(IDataObject dataObject, params string[] formats)
        {
            string data = null;

            foreach (var format in formats)
            {
                data = dataObject.GetData(format) as string;

                if (!string.IsNullOrEmpty(data))
                {
                    break;
                }
            }

            return data;
        }

        private string _MakeSpecialBuferText(string baseString)
        {
            return $"<< {baseString} >>";
        }// TODO (m) is duplicated in BuferHandlersWrapper and BigTextBuferPlugin
    }
}
