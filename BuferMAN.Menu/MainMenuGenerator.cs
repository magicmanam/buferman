using System;
using System.Linq;
using System.Windows.Forms;
using BuferMAN.Clipboard;
using System.Diagnostics;
using System.Deployment.Application;
using System.Reflection;
using magicmanam.UndoRedo;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.Plugins;
using BuferMAN.Infrastructure.Menu;
using System.Threading;
using System.Collections.Generic;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Infrastructure.Files;
using BuferMAN.Infrastructure.Storage;

namespace BuferMAN.Menu
{
    internal class MainMenuGenerator : IMainMenuGenerator
    {
        private readonly IUserFileSelector _userFileSelector;
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IProgramSettingsGetter _settings;
        private readonly IIDataObjectHandler _dataObjectHandler;
        private readonly IEnumerable<IBufermanPlugin> _plugins;
        private readonly IBufermanOptionsWindowFactory _optionsWindowFactory;
        private readonly IFileStorage _fileStorage;
        private readonly ISessionManager _sessionManager;

        public MainMenuGenerator(
            IUserFileSelector userFileSelector,
            IClipboardBuferService clipboardBuferService,
            IProgramSettingsGetter settings,
            IIDataObjectHandler dataObjectHandler,
            IEnumerable<IBufermanPlugin> plugins,
            IBufermanOptionsWindowFactory optionsWindowFactory,
            IFileStorage fileStorage,
            ISessionManager sessionManager)
        {
            this._userFileSelector = userFileSelector;
            this._clipboardBuferService = clipboardBuferService;
            this._settings = settings;
            this._dataObjectHandler = dataObjectHandler;
            this._plugins = plugins;
            this._optionsWindowFactory = optionsWindowFactory;
            this._fileStorage = fileStorage;
            this._sessionManager = sessionManager;
        }

        public IEnumerable<BufermanMenuItem> GenerateMainMenu(IBufermanHost bufermanHost)
        {
            var items = new List<BufermanMenuItem>
            {
                this._GenerateFileMenu(bufermanHost),
                this._GenerateEditMenu(bufermanHost),
                this._GenerateToolsMenu(bufermanHost),
                this._GenerateHelpMenu(bufermanHost)
            };

            return items;
        }

        private BufermanMenuItem _GenerateFileMenu(IBufermanHost bufermanHost)
        {
            var fileMenu = bufermanHost.CreateMenuItem(() => Resource.MenuFile);
            // TODO (l) add menu item "Load bufers from storage" - will open dialog with default storage, or list of files, or other options. Also setting: check storage(s) to load on start
            // Option: reload from storage/file
            fileMenu.AddMenuItem(bufermanHost.CreateMenuItem(() => Resource.MenuFileLoad, (object sender, EventArgs args) => {
                this._userFileSelector.TrySelectBufersStorage(storage => storage.LoadBufers());
            }));
            fileMenu.AddMenuItem(bufermanHost.CreateMenuItem(() => Resource.MenuFileChangeDefault, (object sender, EventArgs args) =>
            {
                var defaultBufersFilePath = this._fileStorage.CombinePaths(
                    this._fileStorage.DataDirectory,
                    this._settings.DefaultBufersFileName);

                Process.Start(defaultBufersFilePath);
            }));

            fileMenu.AddSeparator();

            var pauseResumeMenuItem = bufermanHost.CreateMenuItem(() => (bufermanHost.ShouldCatchCopies ? Resource.MenuFilePause : Resource.MenuFileResume) + $" {new String('\t', 1)} Alt+P");
            pauseResumeMenuItem.AddOnClickHandler((object sender, EventArgs args) =>
            {
                bufermanHost.ShouldCatchCopies = !bufermanHost.ShouldCatchCopies;
            });

            fileMenu.AddMenuItem(pauseResumeMenuItem);

            if (this._sessionManager.IsLatestSessionSaved())
            {
                var restoreSessionMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuFileRestoreSession);
                // no needs to display bufers count from the recent session because cost/benefit ratio is too high

                if (this._settings.RestorePreviousSession)
                {
                    restoreSessionMenuItem.Enabled = false;
                    this._sessionManager.RestoreSession();
                }
                else
                {
                    restoreSessionMenuItem.AddOnClickHandler((object sender, EventArgs args) =>
                    {
                        restoreSessionMenuItem.Enabled = false;
                        this._sessionManager.RestoreSession();
                    });
                    restoreSessionMenuItem.ShortCut = Shortcut.CtrlR;
                }

                fileMenu.AddMenuItem(restoreSessionMenuItem);
            }
            fileMenu.AddSeparator();
            fileMenu.AddMenuItem(bufermanHost.CreateMenuItem(() => Resource.MenuFileExit, (object sender, EventArgs args) => bufermanHost.Exit()));

            return fileMenu;
        }

        private BufermanMenuItem _GenerateEditMenu(IBufermanHost bufermanHost)
        {
            var editMenu = bufermanHost.CreateMenuItem(() => Resource.MenuEdit);

            var undoMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuEditUndo, (sender, args) => { UndoableContext<ApplicationStateSnapshot>.Current.Undo(); bufermanHost.RerenderBufers(this._clipboardBuferService.GetTemporaryBufers(), this._clipboardBuferService.GetPinnedBufers()); });
            undoMenuItem.ShortCut = Shortcut.CtrlZ;
            undoMenuItem.Enabled = false;

            editMenu.AddMenuItem(undoMenuItem);

            var redoMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuEditRedo, (sender, args) => { UndoableContext<ApplicationStateSnapshot>.Current.Redo(); bufermanHost.RerenderBufers(this._clipboardBuferService.GetTemporaryBufers(), this._clipboardBuferService.GetPinnedBufers()); });
            redoMenuItem.ShortCut = Shortcut.CtrlY;
            redoMenuItem.Enabled = false;

            editMenu.AddMenuItem(redoMenuItem);
            var deleteAllMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuEditDel, this._GetOnDeleteAllHandler(bufermanHost));

            editMenu.AddMenuItem(deleteAllMenuItem);

            var deleteTemporaryMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuEditDelTemp, this._OnDeleteAllTemporary(bufermanHost));

            editMenu.AddMenuItem(deleteTemporaryMenuItem);

            editMenu.SetOnPopupHandler((object sender, EventArgs args) =>
            {
                deleteTemporaryMenuItem.Enabled = this._clipboardBuferService.GetTemporaryBufers().Count() > 0;
                deleteAllMenuItem.Enabled = deleteTemporaryMenuItem.Enabled || this._clipboardBuferService.GetPinnedBufers().Count() > 0;
            });

            UndoableContext<ApplicationStateSnapshot>.Current.UndoableAction += (object sender, UndoableActionEventArgs<ApplicationStateSnapshot> e) =>
            {
                undoMenuItem.Enabled = e.CanUndo;
                redoMenuItem.Enabled = e.CanRedo;
            };

            return editMenu;
        }

        private EventHandler _GetOnDeleteAllHandler(IBufermanHost bufermanHost)
        {
            return (object sender, EventArgs args) =>
            {
                if (this._clipboardBuferService.GetPinnedBufers().Any())
                {
                    var result = bufermanHost.UserInteraction.ShowYesNoCancelPopup(Resource.MenuEditDelText, Resource.MenuEditDelTitle);

                    switch (result)
                    {
                        case true:
                            this._clipboardBuferService.RemoveTemporaryClips();
                            break;
                        case false:
                            this._clipboardBuferService.RemoveAllBufers();
                            break;
                    }
                }
                else
                {
                    this._clipboardBuferService.RemoveTemporaryClips();
                }

                bufermanHost.RerenderBufers(this._clipboardBuferService.GetTemporaryBufers(), this._clipboardBuferService.GetPinnedBufers());
            };
        }

        private EventHandler _OnDeleteAllTemporary(IBufermanHost bufermanHost)
        {
           return (object sender, EventArgs args) =>
        {
                this._clipboardBuferService.RemoveTemporaryClips();
                bufermanHost.RerenderBufers(this._clipboardBuferService.GetTemporaryBufers(), this._clipboardBuferService.GetPinnedBufers());
            };
        }

        private BufermanMenuItem _GenerateToolsMenu(IBufermanHost bufermanHost)
        {
            var toolsMenu = bufermanHost.CreateMenuItem(() => Resource.MenuTools);

            toolsMenu.AddMenuItem(bufermanHost.CreateMenuItem(() => Resource.MenuToolsMemory, this._GetShowMemoryUsageHandler(bufermanHost)));
            var pluginsMenuItems = this._GeneratePluginsMenu(bufermanHost);
            if (pluginsMenuItems != null)
            {
                toolsMenu.AddMenuItem(pluginsMenuItems);
            }
            toolsMenu.AddMenuItem(this._GenerateLanguageMenu(bufermanHost));
            toolsMenu.AddMenuItem(bufermanHost.CreateMenuSeparatorItem());
            toolsMenu.AddMenuItem(bufermanHost.CreateMenuItem(() => Resource.MenuToolsOptions, (object sender, EventArgs args) => this._optionsWindowFactory.Create().Open()));

            return toolsMenu;
        }

        private EventHandler _GetShowMemoryUsageHandler(IBufermanHost bufermanHost)
        {// 1) Clear old bufers when memory taken is too much (can be a settings);
            return (object sender, EventArgs args) =>
            {
                using (var performanceCounter = new PerformanceCounter())
                {
                    var proc = Process.GetCurrentProcess();
                    performanceCounter.CategoryName = "Process";
                    performanceCounter.CounterName = "Working Set - Private";
                    performanceCounter.InstanceName = proc.ProcessName;
                    var memorySize = Convert.ToInt32(performanceCounter.NextValue()) / 1024;
                    performanceCounter.Close();

                    bufermanHost.UserInteraction.ShowPopup(string.Format(Resource.MenuToolsMemoryMessageFormat, memorySize), Resource.MenuToolsMemoryCaption);
                }
            };
        }

        private BufermanMenuItem _GeneratePluginsMenu(IBufermanHost bufermanHost)
        {
            var pluginsMenu = bufermanHost.CreateMenuItem(() => Resource.MenuToolsPlugins);

            foreach (var plugin in this._plugins) if (plugin.Available)
                {
                    try
                    {
                        var pluginMenuItem = plugin.CreateMainMenuItem();
                        pluginsMenu.AddMenuItem(pluginMenuItem);
                    }
                    catch (Exception exc)
                    {
                        bufermanHost
                            .UserInteraction
                            .ShowPopup(exc.Message, plugin.Name);
                    }
                }

            if (pluginsMenu.Children.Any())
            {
                pluginsMenu.AddSeparator();
                pluginsMenu.AddMenuItem(bufermanHost.CreateMenuItem(() => Resource.MenuPluginsManagement));// Should open a window to enable/disable, change order (in menu items and so on).

                return pluginsMenu;
            }
            else
            {
                return null;
            }
        }

        private BufermanMenuItem _GenerateLanguageMenu(IBufermanHost bufermanHost)
        {
            // TODO (m) во время открытия приложения показывать диалог с выбором языка и сохранять это значение
            var languageMenu = bufermanHost.CreateMenuItem(() => Resource.MenuToolsLanguage);
            
            var englishMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuToolsLanguageEn);
            languageMenu.AddMenuItem(englishMenuItem);

            var russianMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuToolsLanguageRu);
            languageMenu.AddMenuItem(russianMenuItem);

            switch(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName)
            {
                case "ru":
                    this._UpdateLanguageMenuItems(russianMenuItem, englishMenuItem);
                    break;
                case "en":
                    this._UpdateLanguageMenuItems(englishMenuItem, russianMenuItem);
                    break;
            }

            englishMenuItem.AddOnClickHandler((object sender, EventArgs args) =>
            {
                bufermanHost.ChangeLanguage("en");
                this._UpdateLanguageMenuItems(englishMenuItem, russianMenuItem);
            });
            russianMenuItem.AddOnClickHandler((object sender, EventArgs args) =>
            {
                bufermanHost.ChangeLanguage("ru");
                this._UpdateLanguageMenuItems(russianMenuItem, englishMenuItem);
            });

            return languageMenu;
        }

        private void _UpdateLanguageMenuItems(BufermanMenuItem current, params BufermanMenuItem[] others)
        {
            current.Checked = true;
            current.Enabled = false;

            foreach (var other in others)
            {
                other.Checked = false;
                other.Enabled = true;
            }
        }

        private BufermanMenuItem _GenerateHelpMenu(IBufermanHost buferManHost)
        {
            var helpMenuItem = buferManHost.CreateMenuItem(() => Resource.MenuHelp);

            Func<string> documentationPath = () => Resource.DocumentationPath;
            helpMenuItem.AddMenuItem(buferManHost.CreateMenuItem(() => Resource.DocumentationMenuItem, (object sender, EventArgs e) =>
                Process.Start(documentationPath())));

            //helpMenuItem.AddSeparator();

            var supportMenuItem = buferManHost.CreateMenuItem(() => Resource.MenuHelpSupport);
            supportMenuItem.AddMenuItem(buferManHost.CreateMenuItem("reformal.ru (на русском)", (object sender, EventArgs e) =>
                Process.Start("http://buferman.reformal.ru/")));
            supportMenuItem.AddMenuItem(buferManHost.CreateMenuItem("idea.informer.com (in English)", (object sender, EventArgs e) =>
                Process.Start("http://buferman.idea.informer.com/")));
            supportMenuItem.AddMenuItem(buferManHost.CreateMenuItem(() => Resource.MenuHelpSend, (object sender, EventArgs e) =>
                Process.Start("mailto:magicmanam@gmail.com?subject=BuferMAN")));
            supportMenuItem.AddMenuItem(buferManHost.CreateMenuItem(() => Resource.MenuHelpGitHub, (object sender, EventArgs e) =>
                Process.Start("https://github.com/magicmanam/buferman/issues")));

            helpMenuItem.AddMenuItem(supportMenuItem);

            var donateMenuItem = buferManHost.CreateMenuItem(() => Resource.MenuHelpDonate);
            donateMenuItem.AddMenuItem(buferManHost.CreateMenuItem(() => Resource.MenuHelpDonateIdea, (object sender, EventArgs args) => buferManHost.UserInteraction.ShowPopup(Resource.MenuHelpDonateText, Resource.MenuHelpDonateTitle)));
            donateMenuItem.AddMenuItem(buferManHost.CreateMenuItem("-> klopat.by", (object sender, EventArgs e) =>
                Process.Start("http://www.klopat.by/")));
            helpMenuItem.AddMenuItem(donateMenuItem);

            helpMenuItem.AddSeparator();
            helpMenuItem.AddMenuItem(buferManHost.CreateMenuItem(() => Resource.MenuHelpAbout, (object sender, EventArgs args) => {
                var version = ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion : Assembly.GetEntryAssembly().GetName().Version;

                buferManHost.UserInteraction.ShowPopup(Resource.MenuHelpAboutText + " " + version.ToString(), Resource.MenuHelpAboutTitle); }));

            return helpMenuItem;
        }
    }
}
