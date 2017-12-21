using ClipboardViewer.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logging;
using System.Windows.Forms;
using Windows;
using ClipboardBufer;

namespace ClipboardViewer.Menu
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
            mainMenu.MenuItems.Add(new MenuItem("Edit", new MenuItem[] { new MenuItem("Undo", (sender, args) => { this._clipboardBuferService.Undo(); this._renderingHandler.Render(); }, Shortcut.CtrlZ), new MenuItem("Delete All", OnDeleteAll), new MenuItem("Bufer's Basket", (sender, args) => MessageBox.Show("Available only in Free Pro version.", "Just copy&paste")) }));

            return mainMenu;
        }

        private void OnDeleteAll(object sender, EventArgs args)
        {
            Logger.Write("Delete All");

            var clips = this._clipboardBuferService.GetClips();

            foreach (var clip in clips)
            {
                this._clipboardBuferService.RemoveClip(clip);
            }

            this._renderingHandler.Render();
        }

        //MessageBox.Show("Feature is not supported now. Pay money to support.", "Keep calm and copy&paste!")
    }
}
