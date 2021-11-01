using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Files;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Plugins;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.View;
using Logging;
using magicmanam.UndoRedo;
using magicmanam.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BuferMAN.Application
{
    internal class BufermanApplication : IBufermanApplication
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IClipboardWrapper _clipboardWrapper;
        private readonly IMainMenuGenerator _mainMenuGenerator;
        private readonly IIDataObjectHandler _dataObjectHandler;
        private readonly IBufersStorageFactory _bufersStorageFactory;
        private IEnumerable<BufermanMenuItem> _mainMenuItems;
        private readonly IProgramSettingsGetter _settings;
        private bool _shouldCatchCopies = true;
        private readonly IEnumerable<IBufermanPlugin> _plugins;
        private readonly IBufermanOptionsWindowFactory _optionsWindowFactory;
        private readonly IFileStorage _fileStorage;
        private readonly ISessionManager _sessionManager;
        private DateTime _lastClipboardEventDateTime;
        private readonly ITime _time;

        private event EventHandler<BuferFocusedEventArgs> _BuferFocused;

        public BufermanApplication(IClipboardBuferService clipboardBuferService,
            IClipboardWrapper clipboardWrapper,
            IIDataObjectHandler dataObjectHandler,
            IProgramSettingsGetter settings,
            IMainMenuGenerator mainMenuGenerator,
            IEnumerable<IBufermanPlugin> plugins,
            IBufersStorageFactory bufersStorageFactory,
            IBufermanOptionsWindowFactory optionsWindowFactory,
            IFileStorage fileStorage,
            ITime time,
            ISessionManager sessionManager)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._clipboardWrapper = clipboardWrapper;
            this._plugins = plugins;
            this._mainMenuGenerator = mainMenuGenerator;
            this._dataObjectHandler = dataObjectHandler;
            this._bufersStorageFactory = bufersStorageFactory;
            this._settings = settings;
            this._optionsWindowFactory = optionsWindowFactory;
            this._fileStorage = fileStorage;
            this._time = time;
            this._sessionManager = sessionManager;
        }

        public void RerenderBufers(BufersFilter bufersFilter = null)
        {
            var temporaryBufers = bufersFilter?.BuferType == BuferType.Pinned ?
                Enumerable.Empty<BuferViewModel>() :
                this._clipboardBuferService.GetTemporaryBufers();
            var pinnedBufers = bufersFilter?.BuferType == BuferType.Temporary ?
                Enumerable.Empty<BuferViewModel>() :
                this._clipboardBuferService.GetPinnedBufers();

            if (bufersFilter != null)
            {
                if (bufersFilter.CreatedBefore.HasValue)
                {
                    temporaryBufers = temporaryBufers.Where(b => b.CreatedAt < bufersFilter.CreatedBefore.Value);
                    pinnedBufers = pinnedBufers.Where(b => b.CreatedAt < bufersFilter.CreatedBefore.Value);
                }

                if (bufersFilter.CreatedAfter.HasValue)
                {
                    temporaryBufers = temporaryBufers.Where(b => b.CreatedAt > bufersFilter.CreatedAfter.Value);
                    pinnedBufers = pinnedBufers.Where(b => b.CreatedAt > bufersFilter.CreatedAfter.Value);
                }

                var trimmedFilterText = bufersFilter.Text?.Trim();
                if (trimmedFilterText != string.Empty)
                {
                    temporaryBufers = temporaryBufers
                        .Where(b => (b.TextData?.Contains(trimmedFilterText) ?? false) ||
                          (b.TextRepresentation?.Contains(trimmedFilterText) ?? false) ||
                          (b.Alias?.Contains(trimmedFilterText) ?? false));

                    pinnedBufers = pinnedBufers
                        .Where(b => (b.TextData?.Contains(trimmedFilterText) ?? false) ||
                          (b.TextRepresentation?.Contains(trimmedFilterText) ?? false) ||
                          (b.Alias?.Contains(trimmedFilterText) ?? false));
                }

                if (bufersFilter.ClipboardType == ClipboardType.Image)
                {
                    temporaryBufers = temporaryBufers
                        .Where(b => b.Clip.GetFormats().Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT));

                    pinnedBufers = pinnedBufers
                        .Where(b => b.Clip.GetFormats().Contains(ClipboardFormats.CUSTOM_IMAGE_FORMAT));
                }
            }

            this.Host.RerenderBufers(temporaryBufers, pinnedBufers);
        }

        public void RunInHost(IBufermanHost bufermanHost)
        {
            this.Host = bufermanHost;

            foreach (var plugin in this._plugins)
            {
                try
                {
                    plugin.Initialize(this.Host);
                }
                catch (Exception exc)
                {
                    bufermanHost
                        .UserInteraction
                        .ShowPopup(exc.Message, plugin.Name);
                }
            }
            this._dataObjectHandler.Full += this.Host.OnFullBuferMAN;
            this._dataObjectHandler.Updated += this.Updated;

            this.Host.WindowActivated += this.OnWindowActivating;
            this.Host.ClipbordUpdated += this._ProcessCopyClipboardEvent;
            this.Host.UILanguageChanged += _HostUILanguageChanged;

            UndoableContext<ApplicationStateSnapshot>.Current = new UndoableContext<ApplicationStateSnapshot>(this._clipboardBuferService);

            UndoableContext<ApplicationStateSnapshot>.Current.UndoableAction += (object sender, UndoableActionEventArgs<ApplicationStateSnapshot> e) =>
            {
                if (!e.Action.IsCancelled)
                {
                    if (e.IsRedo)
                    {
                        this.Host.SetStatusBarText(Resource.BuferOperationRestored);
                    }
                    else if (e.IsUndo)
                    {
                        this.Host.SetStatusBarText(Resource.BuferOperationCancelled);
                    }
                    else
                    {
                        this.Host.SetStatusBarText(e.Action.Name);
                    }
                }
            };

            foreach (var storageModel in this._settings.StoragesToLoadOnStart)
            {
                var storage = this._bufersStorageFactory.Create(storageModel);
                storage.LoadBufers();
            }

            this._mainMenuItems = this._mainMenuGenerator.GenerateMainMenu(this, this.Host);
            this.Host.SetMainMenu(this._mainMenuItems);
            this.Host.SetTrayMenu(this.GetTrayMenuItems());

            this.Host.SetOnKeyDown(this.OnKeyDown);
            this._BuferFocused += this.Host.BuferFocused;
        }

        public bool NeedRerender { get; set; }

        private void _ProcessCopyClipboardEvent(object sender, EventArgs e)
        {
            var currentTime = this._time.LocalTime;
            if (this._IsDuplicatedEvent(currentTime))
            {
                return;
            }
            else
            {
                this._lastClipboardEventDateTime = currentTime;
            }

            try
            {
                var dataObject = this._clipboardWrapper.GetDataObject();

                var copy = new DataObject();
                foreach (var format in dataObject.GetFormats())
                {
                    if (format == "EnhancedMetafile")//Fixes bug with copy in Word
                    {
                        copy.SetData(format, "<< !!! EnhancedMetafile !!! >>");
                    }
                    else
                    {
                        try
                        {
                            copy.SetData(format, dataObject.GetData(format));
                        }
                        catch
                        {
                            // TODO (m) Log input parameters and other details.
                        }
                    }
                }

                if (this._clipboardWrapper.ContainsImage())
                {
                    copy.SetData(ClipboardFormats.CUSTOM_IMAGE_FORMAT, this._clipboardWrapper.GetImage());
                }

                var buferViewModel = new BuferViewModel
                {
                    Clip = copy,
                    CreatedAt = this._time.LocalTime
                };

                if (this.ShouldCatchCopies)
                {
                    this._dataObjectHandler.TryHandleDataObject(buferViewModel);
                }
                else
                {
                    using (var action = UndoableContext<ApplicationStateSnapshot>.Current.StartAction())
                    {
                        this._dataObjectHandler.TryHandleDataObject(buferViewModel);

                        action.Cancel();
                    }// TODO (m) Should be refactored
                }

                this.Host.SetStatusBarText(Resource.LastClipboardUpdate + currentTime.ToShortTimeString());
                // TODO (m) Should be in separate strip label
            }
            catch (ExternalException exc)
            {
                Logger.WriteError("An error during get clipboard operation", exc);
                throw new ClipboardMessageException("An error occurred. See logs for more details.", exc);
            }
        }

        /// <summary>
        /// Pseudo fix !!!
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        private bool _IsDuplicatedEvent(DateTime currentTime)
        {
            var delay = currentTime.Ticks - this._lastClipboardEventDateTime.Ticks;

            return delay < 1500000;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Host.HideWindow();
                    break;
                case Keys.Space:
                    new KeyboardEmulator().PressEnter();
                    break;
                case Keys.C:
                    new KeyboardEmulator().PressTab(3);
                    break;
                case Keys.X:
                case Keys.Home:
                    var lastBufer = this._clipboardBuferService.LastTemporaryBufer;
                    if (lastBufer != null)
                    {
                        this._BuferFocused?.Invoke(this, new BuferFocusedEventArgs(lastBufer));
                    }
                    break;
                case Keys.V:
                case Keys.End:
                    var firstBufer = this._clipboardBuferService.FirstPinnedBufer ?? this._clipboardBuferService.FirstTemporaryBufer;

                    if (firstBufer != null)
                    {
                        this._BuferFocused?.Invoke(this, new BuferFocusedEventArgs(firstBufer));
                    }
                    break;
                case Keys.P:
                    if (e.Alt)
                    {
                        this.ShouldCatchCopies = !this.ShouldCatchCopies;
                        this._mainMenuItems = this._mainMenuGenerator.GenerateMainMenu(this, this.Host);
                        this.Host.SetMainMenu(this._mainMenuItems);
                    }
                    break;
            }
        }

        public void Updated(object sender, ClipboardUpdatedEventArgs e)
        {
            this.Host.CurrentBuferViewId = e.ViewModel.ViewId;

            this.NeedRerender = true;
        }

        public void OnWindowActivating(object sender, EventArgs eventArgs)
        {
            this.Host.ActivateWindow();
        }

        public bool ShouldCatchCopies
        {
            get
            {
                return this._shouldCatchCopies;
            }
            set
            {
                if (this._shouldCatchCopies != value)
                {
                    this._shouldCatchCopies = value;
                    this.Host.SetStatusBarText(this._shouldCatchCopies ? Resource.ResumedStatus : Resource.PausedStatus);
                }
            }
        }

        public IBufermanHost Host { get; private set; }

        public IEnumerable<BufermanMenuItem> GetTrayMenuItems()
        {
            var trayIconMenuItems = new List<BufermanMenuItem>
            {
                this.Host.CreateMenuItem(() => Resource.TrayMenuOptions, (object sender, EventArgs args) => this._optionsWindowFactory.Create().Open()),
                this.Host.CreateMenuItem(() => Resource.TrayMenuBuferManual, (object sernder, EventArgs args) => this.Host.UserInteraction.ShowPopup(Resource.UserManual + Environment.NewLine + Environment.NewLine + Resource.DocumentationMentioning, Resource.ApplicationTitle)),
                this.Host.CreateMenuSeparatorItem(),
                this.Host.CreateMenuItem(() => Resource.MenuFileExit, (object sender, EventArgs args) => this.Exit())
            };

            return trayIconMenuItems;
        }

        public string GetBufermanTitle()
        {
            return Resource.ApplicationTitle;
        }

        public string GetBufermanAdminTitle()
        {
            return Resource.AdminApplicationTitle;
        }

        public string GetUserManualText()
        {
            return Resource.UserManual;
        }

        public void RefreshMainMenu()
        {
            foreach (var menuItem in this._mainMenuItems)
            {
                menuItem.TextRefresh();
            }
        }

        private void _HostUILanguageChanged(object sender, EventArgs e)
        {
            this.RefreshMainMenu();
            this.Host.RefreshUI(this._clipboardBuferService.GetTemporaryBufers(), this._clipboardBuferService.GetPinnedBufers());
            this.Host.SetTrayMenu(this.GetTrayMenuItems());
        }

        public void Exit()
        {
            this._sessionManager.SaveSession();
            this.Host.Exit();
        }

        public void ClearEmptyBufers()
        {
            var pinnedBufers = this._clipboardBuferService.GetPinnedBufers();

            var emptyClipFound = false;
            foreach (var bufer in pinnedBufers)
            {
                if (bufer.Clip.IsEmptyObject())
                {
                    emptyClipFound = true;
                    this._RemoveClipWithoutTrackingInUndoableContext(bufer);
                }
            }

            if (emptyClipFound)
            {
                pinnedBufers = this._clipboardBuferService.GetPinnedBufers();
            }

            var temporaryBufers = this._clipboardBuferService.GetTemporaryBufers().ToList();

            do
            {
                emptyClipFound = false;
                var extraTemporaryClipsCount = Math.Max(this._clipboardBuferService.BufersCount - this._settings.MaxBufersCount, 0);
                temporaryBufers = temporaryBufers.Skip(extraTemporaryClipsCount).ToList();

                foreach (var bufer in temporaryBufers)
                {
                    if (bufer.Clip.IsEmptyObject())
                    {
                        emptyClipFound = true;
                        this._RemoveClipWithoutTrackingInUndoableContext(bufer);
                    }
                }

                if (emptyClipFound)
                {
                    temporaryBufers = this._clipboardBuferService.GetTemporaryBufers().ToList();
                }
            } while (emptyClipFound);
        }

        private void _RemoveClipWithoutTrackingInUndoableContext(BuferViewModel bufer)
        {
            ApplicationStateSnapshot stateWithoutEmptyBufer;

            using (var action = UndoableContext<ApplicationStateSnapshot>.Current.StartAction())
            {
                this._clipboardBuferService.RemoveBufer(bufer.ViewId);
                stateWithoutEmptyBufer = this._clipboardBuferService.UndoableState;

                action.Cancel();
            }

            this._clipboardBuferService.UndoableState = stateWithoutEmptyBufer;
        }
    }
}