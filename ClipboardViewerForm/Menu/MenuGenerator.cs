using ClipboardViewerForm.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logging;
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
        }), new MenuItem("SendKeys", (object sender, EventArgs args) => {
            var newText = Microsoft.VisualBasic.Interaction.InputBox($"Enter a new text for this bufer. It can be useful to hide copied passwords or alias some enourmous text. Primary button value was.",
                   "Change bufer's text", "");
            //SendKeys.Send("{^TAB}");
            //SendKeys.Send(newText);
        }) }));
            var undoMenuItem = new MenuItem("Undo", (sender, args) => { this._clipboardBuferService.Undo(); this._renderingHandler.Render(); }, Shortcut.CtrlZ) { Enabled = false };
            var redoMenuItem = new MenuItem("Redo", (sender, args) => { this._clipboardBuferService.CancelUndo(); this._renderingHandler.Render(); }, Shortcut.CtrlY) { Enabled = false };
            mainMenu.MenuItems.Add(new MenuItem("Edit", new MenuItem[] { undoMenuItem, redoMenuItem, new MenuItem("Delete All", OnDeleteAll), new MenuItem("Delete All Temporary", OnDeleteAllTemporary), new MenuItem("Bufer's Basket", (sender, args) => MessageBox.Show("Available only in Free Pro version.", "Just copy&paste")) }));

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
