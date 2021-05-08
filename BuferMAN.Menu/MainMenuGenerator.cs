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
        private readonly IBufersStorageFactory _bufersStorageFactory;

        public MainMenuGenerator(
            IUserFileSelector userFileSelector,
            IClipboardBuferService clipboardBuferService,
            IProgramSettingsGetter settings,
            IIDataObjectHandler dataObjectHandler,
            IEnumerable<IBufermanPlugin> plugins,
            IBufermanOptionsWindowFactory optionsWindowFactory,
            IBufersStorageFactory bufersStorageFactory)
        {
            this._userFileSelector = userFileSelector;
            this._clipboardBuferService = clipboardBuferService;
            this._settings = settings;
            this._dataObjectHandler = dataObjectHandler;
            this._plugins = plugins;
            this._optionsWindowFactory = optionsWindowFactory;
            this._bufersStorageFactory = bufersStorageFactory;
        }

        public void GenerateMainMenu(IBufermanApplication bufermanApplication)
        {
            var items = new List<BufermanMenuItem>
            {
                this._GenerateFileMenu(bufermanApplication),
                this._GenerateEditMenu(bufermanApplication.Host),
                this._GenerateToolsMenu(bufermanApplication),
                this._GenerateHelpMenu(bufermanApplication.Host)
            };

            bufermanApplication.Host.SetMainMenu(items);
        }

        private BufermanMenuItem _GenerateFileMenu(IBufermanApplication bufermanApplication)
        {
            var bufermanHost = bufermanApplication.Host;

            var fileMenu = bufermanHost.CreateMenuItem(Resource.MenuFile);
            // TODO (l) add menu item "Load bufers from storage" - will open dialog with default storage, or list of files, or other options. Also setting: check storage(s) to load on start
            // Option: reload from storage/file
            fileMenu.AddMenuItem(bufermanHost.CreateMenuItem(Resource.MenuFileLoad, (object sender, EventArgs args) => {
                this._userFileSelector.TrySelectBufersStorage(storage => storage.LoadBufers());
            }));
            fileMenu.AddMenuItem(bufermanHost.CreateMenuItem(Resource.MenuFileChangeDefault, (object sender, EventArgs args) =>
            {
                Process.Start(this._settings.DefaultBufersFileName);
            }));

            fileMenu.AddSeparator();

            var pauseResumeMenuItem = bufermanHost.CreateMenuItem(this._GetPauseResumeMenuItemText(bufermanApplication));
            pauseResumeMenuItem.AddOnClickHandler((object sender, EventArgs args) =>
                {
                    bufermanApplication.ShouldCatchCopies = !bufermanApplication.ShouldCatchCopies;
                    pauseResumeMenuItem.Text = this._GetPauseResumeMenuItemText(bufermanApplication);
                });

            fileMenu.AddMenuItem(pauseResumeMenuItem);

            if (bufermanApplication.IsLatestSessionSaved())
            {// TODO (s) Maybe show dialog to ask user to restore previous session? Can be a setting
                var restoreSessionMenuItem = bufermanHost.CreateMenuItem(Resource.MenuFileRestoreSession);// TODO (s) question: do you need bufers count from recent session here?
                restoreSessionMenuItem.AddOnClickHandler((object sender, EventArgs args) =>
                {
                    restoreSessionMenuItem.Enabled = false;// TODO (s) on language change this property is lost (as well as others)!

                    bufermanApplication.RestoreSession();
                });
                restoreSessionMenuItem.ShortCut = Shortcut.CtrlR;

                fileMenu.AddMenuItem(restoreSessionMenuItem);
            }
            fileMenu.AddSeparator();
            fileMenu.AddMenuItem(bufermanHost.CreateMenuItem(Resource.MenuFileExit, (object sender, EventArgs args) => bufermanApplication.Exit()));

            return fileMenu;
        }

        private string _GetPauseResumeMenuItemText(IBufermanApplication bufermanApplication)
        {
            return (bufermanApplication.ShouldCatchCopies ? Resource.MenuFilePause : Resource.MenuFileResume) + $" {new String('\t', 1)} Alt+P";
        }

        private BufermanMenuItem _GenerateEditMenu(IBufermanHost bufermanHost)
        {
            var editMenu = bufermanHost.CreateMenuItem(Resource.MenuEdit);

            var undoMenuItem = bufermanHost.CreateMenuItem(Resource.MenuEditUndo, (sender, args) => { UndoableContext<ApplicationStateSnapshot>.Current.Undo(); bufermanHost.RerenderBufers(); });
            undoMenuItem.ShortCut = Shortcut.CtrlZ;
            undoMenuItem.Enabled = false;

            editMenu.AddMenuItem(undoMenuItem);

            var redoMenuItem = bufermanHost.CreateMenuItem(Resource.MenuEditRedo, (sender, args) => { UndoableContext<ApplicationStateSnapshot>.Current.Redo(); bufermanHost.RerenderBufers(); });
            redoMenuItem.ShortCut = Shortcut.CtrlY;
            redoMenuItem.Enabled = false;

            editMenu.AddMenuItem(redoMenuItem);
            var deleteAllMenuItem = bufermanHost.CreateMenuItem(Resource.MenuEditDel, this._GetOnDeleteAllHandler(bufermanHost));

            editMenu.AddMenuItem(deleteAllMenuItem);

            var deleteTemporaryMenuItem = bufermanHost.CreateMenuItem(Resource.MenuEditDelTemp, this._OnDeleteAllTemporary(bufermanHost));

            editMenu.AddMenuItem(deleteTemporaryMenuItem);

            editMenu.SetOnPopupHandler((object sender, EventArgs args) =>
            {
                deleteTemporaryMenuItem.Enabled = this._clipboardBuferService.GetTemporaryBufers().Count() > 0;
                deleteAllMenuItem.Enabled = deleteTemporaryMenuItem.Enabled || this._clipboardBuferService.GetPinnedBufers().Count() > 0;
            });

            UndoableContext<ApplicationStateSnapshot>.Current.StateChanged += (object sender, UndoableContextChangedEventArgs e) =>
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

                bufermanHost.RerenderBufers();
            };
        }

        private EventHandler _OnDeleteAllTemporary(IBufermanHost bufermanHost)
        {
           return (object sender, EventArgs args) =>
        {
                this._clipboardBuferService.RemoveTemporaryClips();
                bufermanHost.RerenderBufers();
            };
        }

        private BufermanMenuItem _GenerateToolsMenu(IBufermanApplication bufermanApplication)
        {
            var bufermanHost = bufermanApplication.Host;

            var toolsMenu = bufermanHost.CreateMenuItem(Resource.MenuTools);

            toolsMenu.AddMenuItem(bufermanHost.CreateMenuItem(Resource.MenuToolsMemory, this._GetShowMemoryUsageHandler(bufermanHost)));
            toolsMenu.AddMenuItem(this._GeneratePluginsMenu(bufermanHost));
            toolsMenu.AddMenuItem(this._GenerateLanguageMenu(bufermanApplication));
            toolsMenu.AddMenuItem(bufermanHost.CreateMenuSeparatorItem());
            toolsMenu.AddMenuItem(bufermanHost.CreateMenuItem(Resource.MenuToolsOptions, (object sender, EventArgs args) => this._optionsWindowFactory.Create().Open()));

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

        private BufermanMenuItem _GeneratePluginsMenu(IBufermanHost buferManHost)
        {
            var pluginsMenu = buferManHost.CreateMenuItem(Resource.MenuToolsPlugins);

            foreach (var plugin in this._plugins) if (plugin.Enabled)
                {
                    var pluginMenuItem = plugin.CreateMainMenuItem();
                    if (pluginMenuItem != null)
                    {
                        pluginsMenu.AddMenuItem(pluginMenuItem);
                    }
                }
            pluginsMenu.AddSeparator();
            pluginsMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuPluginsManagement));// Should open a window to enable/disable, change order (in menu items and so on).

            return pluginsMenu;
        }

        private BufermanMenuItem _GenerateLanguageMenu(IBufermanApplication bufermanApplication)
        {
            var bufermanHost = bufermanApplication.Host;
            // во время открытия приложения показывать диалог с выбором языка и сохранять это значение
            var languageMenu = bufermanHost.CreateMenuItem(Resource.MenuToolsLanguage);
            
            var englishMenuItem = bufermanHost.CreateMenuItem(Resource.MenuToolsLanguageEn);
            languageMenu.AddMenuItem(englishMenuItem);

            var russianMenuItem = bufermanHost.CreateMenuItem(Resource.MenuToolsLanguageRu);
            languageMenu.AddMenuItem(russianMenuItem);

            switch(Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName)
            {
                case "ru":
                    russianMenuItem.Checked = true;
                    russianMenuItem.Enabled = false;
                    break;
                case "en":
                    englishMenuItem.Checked = true;
                    englishMenuItem.Enabled = false;
                    break;
            }

            englishMenuItem.AddOnClickHandler(this._createLanguageEventHandler("en", bufermanApplication));
            russianMenuItem.AddOnClickHandler(this._createLanguageEventHandler("ru", bufermanApplication));

            return languageMenu;
        }

        private EventHandler _createLanguageEventHandler(string culture, IBufermanApplication bufermanApplication)
        {
            return (object sender, EventArgs args) =>
            {
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);

                this.GenerateMainMenu(bufermanApplication);
                bufermanApplication.Host.RerenderBufers();// TODO (l) this line will work only if I do not use cached button and will recreate all bufers !!!
                bufermanApplication.Host.RerenderUserManual();// TODO (l) make all this rerendering as one method of buferman host object
                bufermanApplication.Host.SetTrayMenu(bufermanApplication.GetTrayMenuItems());
            };
        }

        private BufermanMenuItem _GenerateHelpMenu(IBufermanHost buferManHost)
        {
            var helpMenu = buferManHost.CreateMenuItem(Resource.MenuHelp);
            var startTime = DateTime.Now;
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpSend, (object sender, EventArgs e) =>
                Process.Start("https://rink.hockeyapp.net/apps/51633746a31f44999eca3bc7b7945e92/feedback/new")));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpStats, (object sender, EventArgs args) => buferManHost.UserInteraction.ShowPopup(string.Format(Resource.MenuHelpStatsInfo, startTime, this._dataObjectHandler.CopiesCount, this._dataObjectHandler.CurrentDayCopiesCount), Resource.MenuHelpStatsTitle)));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpDonate, (object sender, EventArgs args) => buferManHost.UserInteraction.ShowPopup(Resource.MenuHelpDonateText, Resource.MenuHelpDonateTitle)));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.DocumentationMenuItem, (object sender, EventArgs e) =>
                Process.Start("Documentation.html")));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem("-> klopat.by", (object sender, EventArgs e) =>
                Process.Start("http://www.klopat.by/")));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpReport, (object sender, EventArgs e) =>
                Process.Start("https://github.com/magicmanam/buferman/issues/new")));
            helpMenu.AddSeparator();
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpAbout, (object sender, EventArgs args) => {
                var version = ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion : Assembly.GetEntryAssembly().GetName().Version;

                buferManHost.UserInteraction.ShowPopup(Resource.MenuHelpAboutText + " " + version.ToString(), Resource.MenuHelpAboutTitle); }));

            return helpMenu;
        }
    }
}
