﻿using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Files;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Plugins;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;
using BuferMAN.View;
using Logging;
using magicmanam.UndoRedo;
using magicmanam.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
        private IBufermanHost _bufermanHost;
        private IEnumerable<BufermanMenuItem> _mainMenuItems;
        private readonly IProgramSettingsGetter _settings;
        private bool _shouldCatchCopies = true;
        private readonly IEnumerable<IBufermanPlugin> _plugins;
        private readonly IBufermanOptionsWindowFactory _optionsWindowFactory;
        private readonly IFileStorage _fileStorage;
        private const string SESSION_FILE_PREFIX = "session_state";
        private DateTime _lastClipboardEventDateTime;
        private ITime _time;

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
            ITime time)
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
            }

            this._bufermanHost.RerenderBufers(temporaryBufers, pinnedBufers);
        }

        public void RunInHost(IBufermanHost bufermanHost)
        {
            this._bufermanHost = bufermanHost;

            foreach (var plugin in this._plugins)
            {
                try
                {
                    plugin.Initialize(this._bufermanHost);
                }
                catch (Exception exc)
                {
                    bufermanHost
                        .UserInteraction
                        .ShowPopup(exc.Message, plugin.Name);
                }
            }
            this._dataObjectHandler.Full += this._bufermanHost.OnFullBuferMAN;
            this._dataObjectHandler.Updated += this.Updated;

            this._bufermanHost.WindowActivated += this.OnWindowActivating;
            this._bufermanHost.ClipbordUpdated += this._ProcessCopyClipboardEvent;

            UndoableContext<ApplicationStateSnapshot>.Current = new UndoableContext<ApplicationStateSnapshot>(this._clipboardBuferService);

            UndoableContext<ApplicationStateSnapshot>.Current.UndoableAction += (object sender, UndoableActionEventArgs<ApplicationStateSnapshot> e) =>
            {
                if (!e.Action.IsCancelled)
                {
                    if (e.IsRedo)
                    {
                        this._bufermanHost.SetStatusBarText(Resource.BuferOperationRestored);
                    }
                    else if (e.IsUndo)
                    {
                        this._bufermanHost.SetStatusBarText(Resource.BuferOperationCancelled);
                    }
                    else
                    {
                        this._bufermanHost.SetStatusBarText(e.Action.Name);
                    }
                }
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

                this._bufermanHost.SetStatusBarText(Resource.LastClipboardUpdate + currentTime.ToShortTimeString());
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
                    this._bufermanHost.HideWindow();
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
            this._bufermanHost.CurrentBuferViewId = e.ViewModel.ViewId;

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

        public void SetMainMenu(IEnumerable<BufermanMenuItem> menuItems)
        {
            this._mainMenuItems = menuItems;

            this.Host.SetMainMenu(menuItems);
        }

        public void RefreshMainMenu()
        {
            foreach (var menuItem in this._mainMenuItems)
            {
                menuItem.TextRefresh();
            }
        }

        public void ChangeLanguage(string shortLanguage)
        {
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(shortLanguage);

            this.RefreshMainMenu();
            this.Host.RefreshUI(this._clipboardBuferService.GetTemporaryBufers(), this._clipboardBuferService.GetPinnedBufers());
            this.Host.SetTrayMenu(this.GetTrayMenuItems());
        }

        public void SaveSession()
        {
            var buferItems = this._clipboardBuferService.GetTemporaryBufers()
                .Where(b => b.Clip.IsStringObject())
                .Select(b => b.ToModel())
                .Union(this._clipboardBuferService
                                   .GetPinnedBufers()
                                   .Where(b => b.Clip.IsStringObject())
                                   .Select(b => b.ToModel()))
                .ToList();
            
            if (buferItems.Any())
            {
                var now = this._time.LocalTime;
                var sessionFile = Path.Combine(this._settings.SessionsRootDirectory, $"{BufermanApplication.SESSION_FILE_PREFIX}_{now.Year}_{now.Month}_{now.Day}_{now.Hour}_{now.Minute}_{now.Second}_{now.Millisecond}_{buferItems.Count()}.json");

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