using System;
using System.Linq;
using System.Windows.Forms;
using BuferMAN.Clipboard;
using System.Diagnostics;
using System.Deployment.Application;
using System.Reflection;
using magicmanam.UndoRedo;
using BuferMAN.Infrastructure;
using BuferMAN.Menu.Properties;
using BuferMAN.Infrastructure.Plugins;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Infrastructure.Menu;
using System.Threading;
using System.Collections.Generic;

namespace BuferMAN.Menu
{
    public class MainMenuGenerator : IMainMenuGenerator
    {
        private readonly ILoadingFileHandler _loadingFileHandler;
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IProgramSettings _settings;
        private readonly IIDataObjectHandler _dataObjectHandler;
        private readonly IEnumerable<IBufermanPlugin> _plugins;

        public MainMenuGenerator(ILoadingFileHandler loadingFileHandler, IClipboardBuferService clipboardBuferService, IProgramSettings settings, IIDataObjectHandler dataObjectHandler, IEnumerable<IBufermanPlugin> plugins)
        {
            this._loadingFileHandler = loadingFileHandler;
            this._clipboardBuferService = clipboardBuferService;
            this._settings = settings;
            this._dataObjectHandler = dataObjectHandler;
            this._plugins = plugins;
        }

        public void GenerateMainMenu(IBufermanHost buferManHost)
        {
            var items = new List<BufermanMenuItem>
            {
                this._GenerateFileMenu(buferManHost),
                this._GenerateEditMenu(buferManHost),
                this._GenerateToolsMenu(buferManHost),
                this._GenerateHelpMenu(buferManHost)
            };

            buferManHost.SetMainMenu(items);
        }

        private BufermanMenuItem _GenerateFileMenu(IBufermanHost buferManHost)
        {
            var fileMenu = buferManHost.CreateMenuItem(Resource.MenuFile);

            fileMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuFileLoad, this._loadingFileHandler.OnLoadFile));
            fileMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuFileChangeDefault, (object sender, EventArgs args) => Process.Start(this._settings.DefaultBufersFileName)));
            fileMenu.AddSeparator();
            fileMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuFileExit, (object sender, EventArgs args) => buferManHost.Exit()));

            return fileMenu;
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
                deleteTemporaryMenuItem.Enabled = this._clipboardBuferService.GetTemporaryClips().Count() > 0;
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

        private BufermanMenuItem _GenerateToolsMenu(IBufermanHost bufermanHost)
        {
            var toolsMenu = bufermanHost.CreateMenuItem(Resource.MenuTools);

            toolsMenu.AddMenuItem(bufermanHost.CreateMenuItem(Resource.MenuToolsMemory, this._GetShowMemoryUsageHandler(bufermanHost)));
            toolsMenu.AddMenuItem(this._GeneratePluginsMenu(bufermanHost));
            toolsMenu.AddMenuItem(this._GenerateLanguageMenu(bufermanHost));

            return toolsMenu;
        }

        private EventHandler _GetShowMemoryUsageHandler(IBufermanHost bufermanHost)
        {// 2) Clear old bufers when memory taken is too much; 3) Number of all bufers (with deleted and not visible)
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

        private BufermanMenuItem _GenerateLanguageMenu(IBufermanHost buferManHost)
        {
            // во время открытия приложения показывать диалог с выбором языка и сохранять это значение
            var languageMenu = buferManHost.CreateMenuItem(Resource.MenuToolsLanguage);
            
            var englishMenuItem = buferManHost.CreateMenuItem(Resource.MenuToolsLanguageEn);
            languageMenu.AddMenuItem(englishMenuItem);

            var russianMenuItem = buferManHost.CreateMenuItem(Resource.MenuToolsLanguageRu);
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

            englishMenuItem.AddOnClickHandler(this._createLanguageEventHandler("en", buferManHost));
            russianMenuItem.AddOnClickHandler(this._createLanguageEventHandler("ru", buferManHost));

            return languageMenu;
        }

        private EventHandler _createLanguageEventHandler(string culture, IBufermanHost buferManHost)
        {
            return (object sender, EventArgs args) =>
            {
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);

                this.GenerateMainMenu(buferManHost);
                // TODO (m) rerender context menu for bufers + short guide at the bottom of main window + tray icon menu
            };
        }

        private BufermanMenuItem _GenerateHelpMenu(IBufermanHost buferManHost)
        {
            var helpMenu = buferManHost.CreateMenuItem(Resource.MenuHelp);
            var startTime = DateTime.Now;
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpSend, (object sender, EventArgs e) =>
                Process.Start("https://rink.hockeyapp.net/apps/51633746a31f44999eca3bc7b7945e92/feedback/new")));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpStats, (object sender, EventArgs args) => buferManHost.UserInteraction.ShowPopup(string.Format(Resource.MenuHelpStatsInfo, startTime, this._dataObjectHandler.CopiesCount), Resource.MenuHelpStatsTitle)));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpDonate, (object sender, EventArgs args) => buferManHost.UserInteraction.ShowPopup(Resource.MenuHelpDonateText, Resource.MenuHelpDonateTitle)));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.DocumentationMenuItem, (object sender, System.EventArgs e) =>
                Process.Start("Documentation.html")));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem("-> klopat.by", (object sender, System.EventArgs e) =>
                Process.Start("http://www.klopat.by/")));
            helpMenu.AddSeparator();
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpAbout, (object sender, EventArgs args) => {
                var version = ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion : Assembly.GetEntryAssembly().GetName().Version;

                buferManHost.UserInteraction.ShowPopup(Resource.MenuHelpAboutText + " " + version.ToString(), Resource.MenuHelpAboutTitle); }));

            return helpMenu;
        }
    }
}
