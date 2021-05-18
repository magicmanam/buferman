using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Files;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Plugins;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;
using BuferMAN.View;
using magicmanam.UndoRedo;
using magicmanam.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IWindowLevelContext _windowLevelContext;
        private IBufermanHost _bufermanHost;
        private readonly IProgramSettingsGetter _settings;
        private bool _shouldCatchCopies = true;
        private readonly IEnumerable<IBufermanPlugin> _plugins;
        private readonly IBufermanOptionsWindowFactory _optionsWindowFactory;
        private readonly IMapper _mapper;
        private readonly IFileStorage _fileStorage;
        private const string SESSION_FILE_PREFIX = "session_state";
        private readonly DateTime _startTime = DateTime.Now;

        private event EventHandler<BuferFocusedEventArgs> _BuferFocused;

        public BufermanApplication(IClipboardBuferService clipboardBuferService,
            IClipboardWrapper clipboardWrapper,
            IIDataObjectHandler dataObjectHandler,
            IProgramSettingsGetter settings,
            IMainMenuGenerator mainMenuGenerator,
            IWindowLevelContext windowLevelContext,
            IEnumerable<IBufermanPlugin> plugins,
            IBufersStorageFactory bufersStorageFactory,
            IBufermanOptionsWindowFactory optionsWindowFactory,
            IMapper mapper,
            IFileStorage fileStorage)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._clipboardWrapper = clipboardWrapper;
            this._plugins = plugins;
            this._mainMenuGenerator = mainMenuGenerator;
            this._dataObjectHandler = dataObjectHandler;
            this._bufersStorageFactory = bufersStorageFactory;
            this._windowLevelContext = windowLevelContext;
            this._settings = settings;
            this._optionsWindowFactory = optionsWindowFactory;
            this._mapper = mapper;
            this._fileStorage = fileStorage;
        }

        public void RunInHost(IBufermanHost bufermanHost)
        {
            this._bufermanHost = bufermanHost;

            foreach (var plugin in this._plugins)
            {
                plugin.Initialize(this._bufermanHost);
            }
            this._dataObjectHandler.Full += this._bufermanHost.OnFullBuferMAN;
            this._dataObjectHandler.Updated += this.Updated;

            this._bufermanHost.WindowActivated += this.OnWindowActivating;
            this._bufermanHost.ClipbordUpdated += this.ProcessCopyClipboardEvent;

            UndoableContext<ApplicationStateSnapshot>.Current = new UndoableContext<ApplicationStateSnapshot>(this._clipboardBuferService);

            UndoableContext<ApplicationStateSnapshot>.Current.UndoableAction += (object sender, UndoableActionEventArgs e) =>
            {
                this._bufermanHost.SetStatusBarText(e.Action);
            };
            UndoableContext<ApplicationStateSnapshot>.Current.UndoAction += (object sender, UndoableActionEventArgs e) =>
            {
                this._bufermanHost.SetStatusBarText(e.Action);
            };
            UndoableContext<ApplicationStateSnapshot>.Current.RedoAction += (object sender, UndoableActionEventArgs e) =>
            {
                this._bufermanHost.SetStatusBarText(e.Action);
            };

            foreach (var storageModel in this._settings.StoragesToLoadOnStart)
            {
                var storage = this._bufersStorageFactory.Create(storageModel);
                storage.LoadBufers();
            }

            this._mainMenuGenerator.GenerateMainMenu(this);
            this.Host.SetTrayMenu(this.GetTrayMenuItems());

            this._bufermanHost.SetOnKeyDown(this.OnKeyDown);
            this._BuferFocused += this._bufermanHost.BuferFocused;

            WindowLevelContext.SetCurrent(this._windowLevelContext);
        }

        public bool NeedRerender { get; set; }

        private void ProcessCopyClipboardEvent(object sender, EventArgs e)
        {
            var dataObject = this._clipboardWrapper.GetDataObject();
            var buferViewModel = new BuferViewModel { Clip = dataObject, CreatedAt = DateTime.Now };

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
                }// Should be refactored
            }

            // TODO (s) maybe in a separate event handler (for example stats plugin)
            if (this._dataObjectHandler.CopiesCount == 100)
            {
                this._bufermanHost.NotificationEmitter.ShowInfoNotification(Resource.NotifyIcon100Congrats, 2500);
            }
            else if (this._dataObjectHandler.CopiesCount == 1000)
            {
                this._bufermanHost.NotificationEmitter.ShowInfoNotification(Resource.NotifyIcon1000Congrats, 2500);
            }

            this._bufermanHost.SetStatusBarText(Resource.LastClipboardUpdate + DateTime.Now.ToShortTimeString());//Should be in separate strip label
            // end of TODO
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    WindowLevelContext.Current.HideWindow();
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
                        this._mainMenuGenerator.GenerateMainMenu(this);
                    }
                    break;
            }
        }

        public void Updated(object sender, ClipboardUpdatedEventArgs e)
        {
            this._bufermanHost.SetCurrentBufer(e.Bufer);

            this.NeedRerender = true;
        }

        public void OnWindowActivating(object sender, EventArgs eventArgs)
        {
            this._bufermanHost.ActivateWindow();
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
                    this._bufermanHost.SetStatusBarText(this._shouldCatchCopies ? Resource.ResumedStatus : Resource.PausedStatus);
                }
            }
        }

        public IBufermanHost Host
        {
            get
            {
                return this._bufermanHost;
            }
        }

        public IEnumerable<BufermanMenuItem> GetTrayMenuItems()
        {
            var trayIconMenuItems = new List<BufermanMenuItem>
            {
                this.Host.CreateMenuItem(Resource.TrayMenuOptions, (object sender, EventArgs args) => this._optionsWindowFactory.Create().Open()),
                this.Host.CreateMenuItem(Resource.TrayMenuBuferManual, (object sernder, EventArgs args) => this.Host.UserInteraction.ShowPopup(Resource.UserManual + Environment.NewLine + Environment.NewLine + Resource.DocumentationMentioning, Resource.ApplicationTitle)),
                this.Host.CreateMenuSeparatorItem(),
                this.Host.CreateMenuItem(Resource.MenuFileExit, (object sender, EventArgs args) => this.Exit())
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

        public void SaveSession()
        {
            var buferItems = this._clipboardBuferService.GetTemporaryBufers()
                .Where(b => b.Clip.IsStringObject())
                .Select(b => this._mapper.Map(b))
                .Union(this._clipboardBuferService
                                   .GetPinnedBufers()
                                   .Where(b => b.Clip.IsStringObject())
                                   .Select(b => this._mapper.Map(b)))
                .ToList();

            if (buferItems.Any())
            {
                var now = DateTime.Now;
                var sessionFile = this._settings.SessionsRootDirectory + $"\\{BufermanApplication.SESSION_FILE_PREFIX}_{now.Year}_{now.Month}_{now.Day}_{now.Hour}_{now.Minute}_{now.Second}_{now.Millisecond}_{buferItems.Count()}.json";

                var storage = this._bufersStorageFactory.CreateStorageByFileExtension(sessionFile);

                storage.SaveBufers(buferItems);
            }
        }

        public bool IsLatestSessionSaved()
        {
            return this._GetLatestSessionSavedFilePath() != null;
        }

        private string _GetLatestSessionSavedFilePath()
        {
            if (this._fileStorage.DirectoryExists(this._settings.SessionsRootDirectory))
            {
                return this._fileStorage.GetFiles(this._settings.SessionsRootDirectory, $"{BufermanApplication.SESSION_FILE_PREFIX}_*.json").Max();
            }
            else
            {
                return null;
            }
        }

        public void RestoreSession()
        {
            var storage = this._bufersStorageFactory.Create(BufersStorageType.JsonFile, this._GetLatestSessionSavedFilePath());
            storage.LoadBufers();
        }

        public void Exit()
        {
            this.SaveSession();
            this._bufermanHost.Exit();
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
            using (var action = UndoableContext<ApplicationStateSnapshot>.Current.StartAction())
            {
                this._clipboardBuferService.RemoveBufer(bufer.ViewId);
                action.Cancel();
            }
        }

        public string GetStatisticsText()
        {
            return this._startTime.Date == DateTime.Now.Date ?
                string.Format(Resource.TodayStatsInfo, this._startTime, this._dataObjectHandler.CurrentDayCopiesCount) :
                string.Format(Resource.StatsInfo, this._startTime, this._dataObjectHandler.CopiesCount, this._dataObjectHandler.CurrentDayCopiesCount);
        }
    }
}