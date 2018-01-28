using ClipboardViewerForm.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Windows;
using ClipboardBufer;
using System.Diagnostics;
using ClipboardViewerForm.Properties;
using System.Deployment.Application;
using System.Reflection;

namespace ClipboardViewerForm.Menu
{
    class MenuGenerator : IMenuGenerator
    {
        private readonly ILoadingFileHandler _loadingFileHandler;
        private readonly IClipboardBuferService _clipboardBuferService;

        public MenuGenerator(ILoadingFileHandler loadingFileHandler, IClipboardBuferService clipboardBuferService)
        {
            this._loadingFileHandler = loadingFileHandler;
            this._clipboardBuferService = clipboardBuferService;
        }
        public MainMenu GenerateMenu(Form form)
        {
            var mainMenu = new MainMenu();
            mainMenu.MenuItems.Add(new MenuItem(Resource.MenuFile, new MenuItem[] { new MenuItem(Resource.MenuFileLoad, this._loadingFileHandler.OnLoadFile), new MenuItem(Resource.MenuFileExit, (object sender, EventArgs args) =>
        {
            WindowsFunctions.SendMessage(form.Handle, Messages.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
        }) }));
            var undoMenuItem = new MenuItem(Resource.MenuEditUndo, (sender, args) => { this._clipboardBuferService.Undo(); WindowLevelContext.Current.RerenderBufers(); }, Shortcut.CtrlZ) { Enabled = false };
            var redoMenuItem = new MenuItem(Resource.MenuEditRedo, (sender, args) => { this._clipboardBuferService.CancelUndo(); WindowLevelContext.Current.RerenderBufers(); }, Shortcut.CtrlY) { Enabled = false };
            mainMenu.MenuItems.Add(new MenuItem(Resource.MenuEdit, new MenuItem[] { undoMenuItem, redoMenuItem, new MenuItem(Resource.MenuEditDel, OnDeleteAll), new MenuItem(Resource.MenuEditDelTemp, OnDeleteAllTemporary) }));

            var startTime = DateTime.Now;
            mainMenu.MenuItems.Add(new MenuItem(Resource.MenuHelp, new MenuItem[] { new MenuItem(Resource.MenuHelpSend, (object sender, EventArgs args) => Process.Start("https://rink.hockeyapp.net/apps/51633746a31f44999eca3bc7b7945e92/feedback/new")), new MenuItem(Resource.MenuHelpStart, (object sender, EventArgs args) => MessageBox.Show(Resource.MenuHelpStartPrefix + $" {startTime}.", Resource.MenuHelpStartTitle)), new MenuItem(Resource.MenuHelpDonate, (object sender, EventArgs args) => MessageBox.Show(Resource.MenuHelpDonateText, Resource.MenuHelpDonateTitle)), new MenuItem(Resource.MenuHelpAbout, (object sender, EventArgs args) => {
            var version = ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion : Assembly.GetEntryAssembly().GetName().Version;

            MessageBox.Show(Resource.MenuHelpAboutText + " " + version.ToString(), Resource.MenuHelpAboutTitle); }) }));

            this._clipboardBuferService.UndoableContextChanged += (object sender, UndoableContextChangedEventArgs e) =>
            {
                undoMenuItem.Enabled = e.CanUndo;
                redoMenuItem.Enabled = e.CanRedo;
            };

            return mainMenu;
        }

        private void OnDeleteAll(object sender, EventArgs args)
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

        private void OnDeleteAllTemporary(object sender, EventArgs args)
        {
            this._clipboardBuferService.RemoveTemporaryClips();
            WindowLevelContext.Current.RerenderBufers();
        }

        //MessageBox.Show("Feature is not supported now. Pay money to support.", "Keep calm and copy&paste!")
        //MessageBox.Show("Available only in Free Pro version.", "Just copy&paste")
        //To keep array of message boxes for not implemented features.
    }
}
