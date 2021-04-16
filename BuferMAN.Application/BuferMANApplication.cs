using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Plugins;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.View;
using magicmanam.UndoRedo;
using magicmanam.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Application
{
    public class BufermanApplication : IBufermanApplication
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IClipboardWrapper _clipboardWrapper;
        private readonly IMainMenuGenerator _mainMenuGenerator;
        private readonly IIDataObjectHandler _dataObjectHandler;
        private readonly IBufersStorageFactory _bufersStorageFactory;
        private readonly IWindowLevelContext _windowLevelContext;
        private IBufermanHost _bufermanHost;
        private readonly IProgramSettings _settings;
        private bool _shouldCatchCopies = true;
        private readonly IEnumerable<IBufermanPlugin> _plugins;

        private event EventHandler<BuferFocusedEventArgs> _BuferFocused;

        public BufermanApplication(IClipboardBuferService clipboardBuferService,
            IClipboardWrapper clipboardWrapper,
            IIDataObjectHandler dataObjectHandler,
            IProgramSettings settings,
            IMainMenuGenerator mainMenuGenerator,
            IWindowLevelContext windowLevelContext,
            IEnumerable<IBufermanPlugin> plugins,
            IBufersStorageFactory bufersStorageFactory)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._clipboardWrapper = clipboardWrapper;
            this._plugins = plugins;
            this._mainMenuGenerator = mainMenuGenerator;
            this._dataObjectHandler = dataObjectHandler;
            this._bufersStorageFactory = bufersStorageFactory;
            this._windowLevelContext = windowLevelContext;
            this._settings = settings;
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

            this._mainMenuGenerator.GenerateMainMenu(this._bufermanHost);

            this._bufermanHost.SetOnKeyDown(this.OnKeyDown);
            this._BuferFocused += this._bufermanHost.BuferFocused;

            WindowLevelContext.SetCurrent(this._windowLevelContext);
        }

        public bool NeedRerender { get; set; }

        private void ProcessCopyClipboardEvent(object sender, EventArgs e)
        {
            var dataObject = this._clipboardWrapper.GetDataObject();
            var buferViewModel = new BuferViewModel { Clip = dataObject, CreatedAt = DateTime.Now };

            if (buferViewModel.Clip.GetData("System.String") as string == "")
            {
                this._bufermanHost.UserInteraction.ShowPopup($"This bufer should be inspected: System.String format is empty", "BuferMAN");
            }// TODO (s) remove this line at July

            if (this._shouldCatchCopies)
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
                        this._shouldCatchCopies = !this._shouldCatchCopies;
                        this._bufermanHost.SetStatusBarText(this._shouldCatchCopies ? Resource.ResumedStatus : Resource.PausedStatus);
                    }
                    break;
            }
        }

        public void Updated(object sender, ClipboardUpdatedEventArgs e)
        {
            this._bufermanHost.SetCurrentBufer(e.Bufer);

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
            this._bufermanHost.ActivateWindow();

            if (this.NeedRerender)
            {
                this._bufermanHost.RerenderBufers();
                this.NeedRerender = false;
            }
        }
    }
}