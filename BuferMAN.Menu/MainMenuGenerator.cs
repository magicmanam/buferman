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
using BuferMAN.Menu.Plugins;
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

        public MainMenuGenerator(ILoadingFileHandler loadingFileHandler, IClipboardBuferService clipboardBuferService, IProgramSettings settings)
        {
            this._loadingFileHandler = loadingFileHandler;
            this._clipboardBuferService = clipboardBuferService;
            this._settings = settings;
        }

        public void GenerateMainMenu(IBuferMANHost buferManHost)
        {
            var items = new List<BuferMANMenuItem>
            {
                this._GenerateFileMenu(buferManHost),
                this._GenerateEditMenu(buferManHost),
                this._GenerateToolsMenu(buferManHost),
                this._GenerateHelpMenu(buferManHost)
            };

            buferManHost.SetMainMenu(items);
        }

        private BuferMANMenuItem _GenerateFileMenu(IBuferMANHost buferManHost)
        {
            var fileMenu = buferManHost.CreateMenuItem(Resource.MenuFile);

            fileMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuFileLoad, this._loadingFileHandler.OnLoadFile));
            fileMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuFileChangeDefault, (object sender, EventArgs args) => Process.Start(this._settings.DefaultBufersFileName)));
            fileMenu.AddSeparator();
            fileMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuFileExit, (object sender, EventArgs args) => buferManHost.Exit()));

            return fileMenu;
        }

        private BuferMANMenuItem _GenerateEditMenu(IBuferMANHost buferMANHost)
        {
            var editMenu = buferMANHost.CreateMenuItem(Resource.MenuEdit);

            var undoMenuItem = buferMANHost.CreateMenuItem(Resource.MenuEditUndo, (sender, args) => { UndoableContext<ApplicationStateSnapshot>.Current.Undo(); WindowLevelContext.Current.RerenderBufers(); });
            undoMenuItem.ShortCut = Shortcut.CtrlZ;
            undoMenuItem.Enabled = false;

            editMenu.AddMenuItem(undoMenuItem);

            var redoMenuItem = buferMANHost.CreateMenuItem(Resource.MenuEditRedo, (sender, args) => { UndoableContext<ApplicationStateSnapshot>.Current.Redo(); WindowLevelContext.Current.RerenderBufers(); });
            redoMenuItem.ShortCut = Shortcut.CtrlY;
            redoMenuItem.Enabled = false;

            editMenu.AddMenuItem(redoMenuItem);
            var deleteAllMenuItem = buferMANHost.CreateMenuItem(Resource.MenuEditDel, this._OnDeleteAll);

            editMenu.AddMenuItem(deleteAllMenuItem);

            var deleteTemporaryMenuItem = buferMANHost.CreateMenuItem(Resource.MenuEditDelTemp, this._OnDeleteAllTemporary);

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

        private void _OnDeleteAll(object sender, EventArgs args)
        {
            if (this._clipboardBuferService.GetPinnedBufers().Any())
            {
                var result = MessageBox.Show(Resource.MenuEditDelText, Resource.MenuEditDelTitle, MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Yes)
                {
                    this._clipboardBuferService.RemoveTemporaryClips();
                }
                else if (result == DialogResult.No)
                {
                    this._clipboardBuferService.RemoveAllBufers();
                }
            }
            else
            {
                this._clipboardBuferService.RemoveTemporaryClips();
            }

            WindowLevelContext.Current.RerenderBufers();
        }

        private void _OnDeleteAllTemporary(object sender, EventArgs args)
        {
            this._clipboardBuferService.RemoveTemporaryClips();
            WindowLevelContext.Current.RerenderBufers();
        }

        private BuferMANMenuItem _GenerateToolsMenu(IBuferMANHost buferManHost)
        {
            var toolsMenu = buferManHost.CreateMenuItem(Resource.MenuTools);

            toolsMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuToolsMemory));// 1) Show taken memory; 2) Clear old bufers when memory taken is too much; 3) Number of all bufers (with deleted and not visible)
            toolsMenu.AddMenuItem(this._GeneratePluginsMenu(buferManHost));
            toolsMenu.AddMenuItem(this._GenerateLanguageMenu(buferManHost));

            return toolsMenu;
        }

        private BuferMANMenuItem _GeneratePluginsMenu(IBuferMANHost buferManHost)
        {
            var pluginsMenu = buferManHost.CreateMenuItem(Resource.MenuToolsPlugins);

            pluginsMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuPluginsScripts));
            pluginsMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuPluginsPCCleaner));

            var status = SystemInformation.PowerStatus;
            if ((status.BatteryChargeStatus & (BatteryChargeStatus.NoSystemBattery | BatteryChargeStatus.Unknown)) != 0)
            {
                var factory = new BatterySaverMenuItemFactory(buferManHost);// TODO : into DI constructor and implement via plugins 
                pluginsMenu.AddMenuItem(factory.Create());
            }

            return pluginsMenu;
        }

        private BuferMANMenuItem _GenerateLanguageMenu(IBuferMANHost buferManHost)
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

            englishMenuItem.SetOnClickHandler(this._createLanguageEventHandler("en", buferManHost));
            russianMenuItem.SetOnClickHandler(this._createLanguageEventHandler("ru", buferManHost));

            return languageMenu;
        }

        private EventHandler _createLanguageEventHandler(string culture, IBuferMANHost buferManHost)
        {
            return (object sender, EventArgs args) =>
            {
                Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);

                this.GenerateMainMenu(buferManHost);
                // rerender context menu for bufers + short guide at the bottom of main window
            };
        }

        private BuferMANMenuItem _GenerateHelpMenu(IBuferMANHost buferManHost)
        {
            var helpMenu = buferManHost.CreateMenuItem(Resource.MenuHelp);
            var startTime = DateTime.Now;
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpSend, (object sender, EventArgs e) =>
                Process.Start("https://rink.hockeyapp.net/apps/51633746a31f44999eca3bc7b7945e92/feedback/new")));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpStart, (object sender, EventArgs args) => MessageBox.Show(Resource.MenuHelpStartPrefix + $" {startTime}.", Resource.MenuHelpStartTitle)));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpDonate, (object sender, EventArgs args) => MessageBox.Show(Resource.MenuHelpDonateText, Resource.MenuHelpDonateTitle)));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.DocumentationMenuItem, (object sender, System.EventArgs e) =>
                Process.Start("Documentation.html")));
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem("-> klopat.by", (object sender, System.EventArgs e) =>
                Process.Start("http://www.klopat.by/")));
            helpMenu.AddSeparator();
            helpMenu.AddMenuItem(buferManHost.CreateMenuItem(Resource.MenuHelpAbout, (object sender, EventArgs args) => {
                var version = ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion : Assembly.GetEntryAssembly().GetName().Version;

                MessageBox.Show(Resource.MenuHelpAboutText + " " + version.ToString(), Resource.MenuHelpAboutTitle); }));

            return helpMenu;
        }
    }
}
