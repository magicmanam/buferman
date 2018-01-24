using ClipboardViewerForm.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Windows;
using ClipboardBufer;

namespace ClipboardViewerForm.Menu
{
    class MenuGenerator : IMenuGenerator
    {
        private readonly ILoadingFileHandler _loadingFileHandler;
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IRenderingHandler _renderingHandler;
        
        public MenuGenerator(ILoadingFileHandler loadingFileHandler, IClipboardBuferService clipboardBuferService, IRenderingHandler renderingHandler)
        {
            this._loadingFileHandler = loadingFileHandler;
            this._clipboardBuferService = clipboardBuferService;
            this._renderingHandler = renderingHandler;
        }
        public MainMenu GenerateMenu(Form form)
        {
            var mainMenu = new MainMenu();
            mainMenu.MenuItems.Add(new MenuItem("File", new MenuItem[] { new MenuItem("Load from file", this._loadingFileHandler.OnLoadFile), new MenuItem("Exit session", (object sender, EventArgs args) =>
        {
            WindowsFunctions.SendMessage(form.Handle, Messages.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
        }) }));
            var undoMenuItem = new MenuItem("Undo", (sender, args) => { this._clipboardBuferService.Undo(); this._renderingHandler.Render(); }, Shortcut.CtrlZ) { Enabled = false };
            var redoMenuItem = new MenuItem("Redo", (sender, args) => { this._clipboardBuferService.CancelUndo(); this._renderingHandler.Render(); }, Shortcut.CtrlY) { Enabled = false };
            mainMenu.MenuItems.Add(new MenuItem("Edit", new MenuItem[] { undoMenuItem, redoMenuItem, new MenuItem("Delete all", OnDeleteAll), new MenuItem("Delete all temporary", OnDeleteAllTemporary), new MenuItem("Bufer's basket", (sender, args) => MessageBox.Show("Available only in Free Pro version.", "Just copy&paste")) }));

            var startTime = DateTime.Now;
            mainMenu.MenuItems.Add(new MenuItem("Help", new MenuItem[] { new MenuItem("Send feedback"), new MenuItem("Start time", (object sender, EventArgs args) => MessageBox.Show($"Program was started at {startTime}.", "Start time")), new MenuItem("Donate", (object sender, EventArgs args) => MessageBox.Show("Hello my Friend! Thank you for interest to this program! It was created with love to help other people in their day-to-day activities. The most valuable gift for me will be your help for other people! There are many people who need your help and support around: people with disabilities, children without parents and so on... You can find any clarity organization or smthg similar and transfer there some funds - I will really appreciate your act! Thank you again and have a nice day!", "Your help is really appreciated")), new MenuItem("About") }));

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
                var result = MessageBox.Show("There are persistent bufers exist. Do you want to delete only temporal bufers?", "Confirm persistent bufers deletion", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    this._clipboardBuferService.RemoveTemporaryClips();
                } else
                {
                    this._clipboardBuferService.RemoveAllClips();
                }
            }
            else
            {
                this._clipboardBuferService.RemoveTemporaryClips();
            }

            this._renderingHandler.Render();
        }

        private void OnDeleteAllTemporary(object sender, EventArgs args)
        {
            this._clipboardBuferService.RemoveTemporaryClips();
            this._renderingHandler.Render();
        }

        //MessageBox.Show("Feature is not supported now. Pay money to support.", "Keep calm and copy&paste!")
        //To keep array of message boxes for not implemented features.
    }
}
