using System;
using System.Linq;
using System.Windows.Forms;
using Windows;
using ClipboardBufer;
using System.Diagnostics;
using ClipboardViewerForm.Properties;
using System.Deployment.Application;
using System.Reflection;
using BuferMAN.Menu.Help;

namespace ClipboardViewerForm.Menu
{
    class MenuGenerator : IMenuGenerator
    {
        private readonly ILoadingFileHandler _loadingFileHandler;
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IProgramSettings _settings;

        public MenuGenerator(ILoadingFileHandler loadingFileHandler, IClipboardBuferService clipboardBuferService, IProgramSettings settings)
        {
            this._loadingFileHandler = loadingFileHandler;
            this._clipboardBuferService = clipboardBuferService;
            this._settings = settings;
        }
        public MainMenu GenerateMenu()
        {
            var menu = new MainMenu();
            menu.MenuItems.Add(this._GenerateFileMenu());
            menu.MenuItems.Add(this._GenerateEditMenu());
            menu.MenuItems.Add(this._GenerateHelpMenu());

            return menu;
        }

        private MenuItem _GenerateFileMenu()
        {
            var fileMenu = new MenuItem(Resource.MenuFile);

            fileMenu.MenuItems.Add(new MenuItem(Resource.MenuFileLoad, this._loadingFileHandler.OnLoadFile));
            fileMenu.MenuItems.Add(new MenuItem(Resource.MenuFileChangeDefault, (object sender, EventArgs args) => Process.Start(this._settings.DefaultBufersFileName)));
            fileMenu.MenuItems.AddSeparator();
            fileMenu.MenuItems.Add(new MenuItem(Resource.MenuFileExit, (object sender, EventArgs args) => WindowsFunctions.SendMessage(WindowLevelContext.Current.WindowHandle, Messages.WM_DESTROY, IntPtr.Zero, IntPtr.Zero)));

            return fileMenu;
        }

        private MenuItem _GenerateEditMenu()
        {
            var editMenu = new MenuItem(Resource.MenuEdit);

            var undoMenuItem = new MenuItem(Resource.MenuEditUndo, (sender, args) => { this._clipboardBuferService.Undo(); WindowLevelContext.Current.RerenderBufers(); }, Shortcut.CtrlZ) { Enabled = false };
            editMenu.MenuItems.Add(undoMenuItem);
            var redoMenuItem = new MenuItem(Resource.MenuEditRedo, (sender, args) => { this._clipboardBuferService.CancelUndo(); WindowLevelContext.Current.RerenderBufers(); }, Shortcut.CtrlY) { Enabled = false };
            editMenu.MenuItems.Add(redoMenuItem);
            editMenu.MenuItems.Add(new MenuItem(Resource.MenuEditDel, _OnDeleteAll));
            editMenu.MenuItems.Add(new MenuItem(Resource.MenuEditDelTemp, _OnDeleteAllTemporary));

            this._clipboardBuferService.UndoableContextChanged += (object sender, UndoableContextChangedEventArgs e) =>
            {
                undoMenuItem.Enabled = e.CanUndo;
                redoMenuItem.Enabled = e.CanRedo;
            };

            return editMenu;
        }

        private void _OnDeleteAll(object sender, EventArgs args)
        {
            if (this._clipboardBuferService.GetPersistentClips().Any())
            {
                var result = MessageBox.Show(Resource.MenuEditDelText, Resource.MenuEditDelTitle, MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    this._clipboardBuferService.RemoveTemporaryClips();
                }
                else
                {
                    this._clipboardBuferService.RemoveAllClips();
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

        private MenuItem _GenerateHelpMenu()
        {
            var helpMenu = new MenuItem(Resource.MenuHelp);
            var startTime = DateTime.Now;
            helpMenu.MenuItems.Add(new MenuItem(Resource.MenuHelpSend, (object sender, EventArgs args) => Process.Start("https://rink.hockeyapp.net/apps/51633746a31f44999eca3bc7b7945e92/feedback/new")));
            helpMenu.MenuItems.Add(new MenuItem(Resource.MenuHelpStart, (object sender, EventArgs args) => MessageBox.Show(Resource.MenuHelpStartPrefix + $" {startTime}.", Resource.MenuHelpStartTitle)));
            helpMenu.MenuItems.Add(new MenuItem(Resource.MenuHelpDonate, (object sender, EventArgs args) => MessageBox.Show(Resource.MenuHelpDonateText, Resource.MenuHelpDonateTitle)));
            helpMenu.MenuItems.Add(new DocumentationMenuItem());
            helpMenu.MenuItems.Add(new MenuItem(Resource.MenuHelpAbout, (object sender, EventArgs args) => {
                var version = ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion : Assembly.GetEntryAssembly().GetName().Version;

                MessageBox.Show(Resource.MenuHelpAboutText + " " + version.ToString(), Resource.MenuHelpAboutTitle); }));

            return helpMenu;
        }
    }
}
