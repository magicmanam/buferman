using BuferMAN.Application.Properties;
using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Plugins;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Storage;
using BuferMAN.View;
using magicmanam.UndoRedo;
using magicmanam.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace BuferMAN.Application
{
    public class BuferMANApplication
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IClipboardWrapper _clipboardWrapper;
        private readonly ILoadingFileHandler _loadingFileHandler;
        private readonly IIDataObjectHandler _dataObjectHandler;
        private readonly IBufermanHost _bufermanHost;
        private readonly IProgramSettings _settings;
        private readonly BuferItemDataObjectConverter _buferItemDataObjectConverter = new BuferItemDataObjectConverter();
        private long _copiesCount = 0;
        private bool _shouldCatchCopies = true;
        private readonly IEnumerable<IBufermanPlugin> _plugins;

        private event EventHandler<BuferFocusedEventArgs> _BuferFocused;

        public BuferMANApplication(IBufermanHost bufermanHost, IClipboardBuferService clipboardBuferService, IClipboardWrapper clipboardWrapper, ILoadingFileHandler loadingFileHandler, IIDataObjectHandler dataObjectHandler, IProgramSettings settings, IMainMenuGenerator menuGenerator, IWindowLevelContext windowLevelContext,
            IEnumerable<IBufermanPlugin> plugins)
        {
            this._bufermanHost = bufermanHost;
            this._clipboardBuferService = clipboardBuferService;
            this._clipboardWrapper = clipboardWrapper;
            this._plugins = plugins;

            foreach (var plugin in this._plugins)
            {
                plugin.InitializeHost(this._bufermanHost);
            }

            this._loadingFileHandler = loadingFileHandler;
            this._loadingFileHandler.BufersLoaded += this._loadingFileHandler_BufersLoaded;

            this._dataObjectHandler = dataObjectHandler;
            this._dataObjectHandler.Full += this._bufermanHost.OnFullBuferMAN;
            this._dataObjectHandler.Updated += this.Updated;

            this._bufermanHost.WindowActivated += this.OnWindowActivating;
            this._bufermanHost.ClipbordUpdated += this.ProcessCopyClipboardEvent;

            this._settings = settings;
            // TODO (s) relocate the code below into .Start method
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

            if (File.Exists(settings.DefaultBufersFileName))
            {
                this.LoadBufersFromStorage();
            }

            menuGenerator.GenerateMainMenu(bufermanHost);

            bufermanHost.SetOnKeyDown(this.OnKeyDown);
            this._BuferFocused += bufermanHost.BuferFocused;

            WindowLevelContext.SetCurrent(windowLevelContext);
        }

        public bool NeedRerender { get; set; }

        private void ProcessCopyClipboardEvent(object sender, EventArgs e)
        {
            if (this._shouldCatchCopies)
            {
                this._copiesCount++;
                var dataObject = this._clipboardWrapper.GetDataObject();
                this._dataObjectHandler.TryHandleDataObject(new BuferViewModel { Clip = dataObject, CreatedAt = DateTime.Now });

                if (this._copiesCount == 100)
                {
                    this._bufermanHost.NotificationEmitter.ShowInfoNotification(Resource.NotifyIcon100Congrats, 2500);
                }
                else if (this._copiesCount == 1000)
                {
                    this._bufermanHost.NotificationEmitter.ShowInfoNotification(Resource.NotifyIcon1000Congrats, 2500);
                }

                this._bufermanHost.SetStatusBarText(Resource.LastClipboardUpdate + DateTime.Now.ToShortTimeString());//Should be in separate strip label
            }
        }

        public void LoadBufersFromStorage()
        {
            this._loadingFileHandler.LoadBufersFromFile(_settings.DefaultBufersFileName);
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
                        this._shouldCatchCopies = !this._shouldCatchCopies;
                        this._bufermanHost.SetStatusBarText(this._shouldCatchCopies ? Resource.ResumedStatus : Resource.PausedStatus);
                    }
                    break;
            }
        }

        public void Updated(object sender, EventArgs e)
        {
            if (this._bufermanHost.IsVisible)
            {
                this._bufermanHost.RerenderBufers();
                this.NeedRerender = false;
            }
            else
            {
                this.NeedRerender = true;
            }
        }

        public void OnWindowActivating(object sender, EventArgs eventArgs)
        {
            WindowLevelContext.Current.ActivateWindow();

            if (this.NeedRerender)
            {
                WindowLevelContext.Current.RerenderBufers();
                this.NeedRerender = false;
            }
        }

        private void _loadingFileHandler_BufersLoaded(object sender, BufersLoadedEventArgs e)
        {
            using (var action = UndoableContext<ApplicationStateSnapshot>.Current.StartAction())
            {
                var loaded = false;

                foreach (var bufer in e.Bufers)
                {
                    var dataObject = this._buferItemDataObjectConverter.ToDataObject(bufer);
                    var buferViewModel = new BuferViewModel
                    {
                        Clip = dataObject,
                        Alias = bufer.Alias,
                        CreatedAt = DateTime.Now,
                        Pinned = bufer.Pinned
                    };

                    var tempLoaded = this._dataObjectHandler.TryHandleDataObject(buferViewModel);
                    loaded = tempLoaded || loaded;
                }

                if (!loaded)
                {
                    action.Cancel();
                }
            }
        }
    }
}
