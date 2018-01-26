using ClipboardBufer;
using ClipboardViewerForm.Properties;
using ClipboardViewerForm.Window;
using Microsoft.VisualBasic;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Windows;

namespace ClipboardViewerForm.ClipMenu
{
    class ClipMenuGenerator : IClipMenuGenerator
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IRenderingHandler _renderingHandler;
        private readonly IBuferSelectionHandler _buferSelectionHandler;
        private readonly IWindowHidingHandler _hidingHandler;
        private IDataObject _dataObject;
        private Button _button;
        private MenuItem _returnTextToInitialMenuItem;
        private MenuItem _markAsPersistentMenuItem;
        private MenuItem _createLoginDataMenuItem;
        private MenuItem _addToFileMenuItem;
        private MenuItem _pasteMenuItem;
        private String _originBuferText;
        private string _tooltipText;
        private ToolTip _mouseOverTooltip;

        public ClipMenuGenerator(IClipboardBuferService clipboardBuferService, IRenderingHandler renderingHandler, BuferSelectionHandler buferSelectionHandler, IWindowHidingHandler hidingHandler)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._renderingHandler = renderingHandler;
            this._buferSelectionHandler = buferSelectionHandler;
            this._hidingHandler = hidingHandler;
        }

        public ContextMenu GenerateContextMenu(IDataObject dataObject, Button button, String originBuferText, string tooltipText, ToolTip mouseOverTooltip, bool isChangeTextAvailable)
        {
            this._dataObject = dataObject;
            this._button = button;
            this._originBuferText = originBuferText;
            this._tooltipText = tooltipText;
            this._mouseOverTooltip = mouseOverTooltip;

            var contextMenu = new ContextMenu();

            this._markAsPersistentMenuItem = new MenuItem(Resource.MenuPersistent, this.MarkAsPersistent);
            contextMenu.MenuItems.Add(this._markAsPersistentMenuItem);
            
            var formats = this._dataObject.GetFormats();
            var formatsMenu = new MenuItem();
            var formatsCount = formats.Length;
            
            formatsMenu.Shortcut = Shortcut.AltDownArrow;
            foreach (var format in this._dataObject.GetFormats())
            {
                if (format != ClipboardFormats.CUSTOM_IMAGE_FORMAT)
                {
                    var particularFormatMenu = new MenuItem(format);
                    particularFormatMenu.Click += (object sender, EventArgs args) =>
                    {
                        MessageBox.Show(this._dataObject.GetData(format).ToString(), format);
                    };
                    formatsMenu.MenuItems.Add(particularFormatMenu);
                }
                else
                {
                    formatsCount -= 1;
                }
            }

            formatsMenu.Text = Resource.MenuFormats + $" ({formatsCount})";

            contextMenu.MenuItems.Add(formatsMenu);
            contextMenu.MenuItems.Add(new MenuItem(Resource.MenuDelete, this.DeleteBufer, Shortcut.Del));

            this._pasteMenuItem = new MenuItem(Resource.MenuPaste + $" {new String('\t', 4)} Enter", (object sender, EventArgs ars) =>
            {
                new KeyboardEmulator().PressEnter();
            });
            contextMenu.MenuItems.Add(this._pasteMenuItem);
            
            if (isChangeTextAvailable)
            {
                contextMenu.MenuItems.Add("-");
                contextMenu.MenuItems.Add(new MenuItem(Resource.MenuCharByChar, (object sender, EventArgs args) => {
                    this._hidingHandler.HideWindow();
                    new KeyboardEmulator().TypeText(this._originBuferText);
                }));

                this._returnTextToInitialMenuItem = new MenuItem(Resource.MenuReturn, ReturnTextToInitial) { Enabled = false };
                contextMenu.MenuItems.Add(_returnTextToInitialMenuItem);
                contextMenu.MenuItems.Add(new MenuItem(Resource.MenuChange, this.ChangeText));

                this._addToFileMenuItem = new MenuItem(Resource.MenuAddToFile, (object sender, EventArgs args) =>
                {
                    using (var sw = new StreamWriter(new FileStream("bufers.txt", FileMode.Append, FileAccess.Write)))
                    {
                        sw.WriteLine(this._originBuferText);
                    }
                    this._addToFileMenuItem.Text = Resource.MenuAddedToFile;
                    this._addToFileMenuItem.Enabled = false;
                }, Shortcut.CtrlF);
                contextMenu.MenuItems.Add(this._addToFileMenuItem);

                this._createLoginDataMenuItem = new MenuItem(Resource.MenuCreds, this._CreateLoginCredentials);
                contextMenu.MenuItems.Add(this._createLoginDataMenuItem);
            }

            return contextMenu;
        }

        private void DeleteBufer(object sender, EventArgs e)
        {
            this._clipboardBuferService.RemoveClip(this._dataObject);
            this._renderingHandler.Render();
        }

        private void MarkAsPersistent(object sender, EventArgs e)
        {
            this._clipboardBuferService.MarkClipAsPersistent(this._dataObject);
            this._markAsPersistentMenuItem.Enabled = false;
            this._renderingHandler.Render();
        }

        private void _CreateLoginCredentials(object sender, EventArgs e)
        {
            var password = Interaction.InputBox(Resource.CreateCredsPrefix + $" \"{this._originBuferText}\". " + Resource.CreateCredsPostfix,
                  Resource.CreateCredsTitle,
                   null);

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(Resource.EmptyPasswordError);
            } else
            {
                this._createLoginDataMenuItem.Text = Resource.LoginCreds;
                this._pasteMenuItem.Text = Resource.LoginWith + $" {new String('\t', 2)} Enter";
                this._dataObject.SetData(ClipboardFormats.PASSWORD_FORMAT, password);
                this.TryChangeText(Resource.CredsPrefix + $" {this._button.Text}");

                this._button.Click -= this._buferSelectionHandler.DoOnClipSelection;
                this._button.Click += (object pasteCredsSender, EventArgs args) =>
                {
                    this._hidingHandler.HideWindow();

                    new KeyboardEmulator()
                        .TypeText(this._originBuferText)
                        .PressTab()
                        .TypeText(password)
                        .PressEnter();
                };

                this._createLoginDataMenuItem.MenuItems.Add(new MenuItem(Resource.CredsPasswordEnter, (object pastePasswordSender, EventArgs args) =>
                {
                    this._hidingHandler.HideWindow();
                    new KeyboardEmulator()
                        .TypeText(password)
                        .PressEnter();
                }));
                this._createLoginDataMenuItem.MenuItems.Add(new MenuItem(Resource.CredsPassword, (object pastePasswordSender, EventArgs args) =>
                {
                    this._hidingHandler.HideWindow();

                    new KeyboardEmulator()
                        .TypeText(password);
                }));
                this._createLoginDataMenuItem.MenuItems.Add(new MenuItem(Resource.CredsName, (object pasteUsernameSender, EventArgs args) =>
                {
                    this._hidingHandler.HideWindow();

                    new KeyboardEmulator()
                        .TypeText(this._originBuferText);
                }));
                MarkAsPersistent(sender, e);
            }
        }

        private void ChangeText(object sender, EventArgs e)
        {
            var newText = Interaction.InputBox(Resource.ChangeTextPrefix + $" \"{this._originBuferText}\". " + Resource.ChangeTextPostfix,
                   Resource.ChangeTextTitle,
                   this._button.Text);

            this.TryChangeText(newText);
        }

        private void TryChangeText(string newText)
        {
            if (!string.IsNullOrWhiteSpace(newText) && newText != this._button.Text)
            {
                this._button.Text = newText;
                this._tooltipText = newText;
                bool isOriginText = newText == this._originBuferText;

                if (isOriginText)
                {
                    this._button.Font = new Font(this._button.Font, FontStyle.Regular);
                    MessageBox.Show(Resource.BuferAliasReturned);

                }
                else
                {
                    this._button.Font = new Font(this._button.Font, FontStyle.Bold);
                }

                this._returnTextToInitialMenuItem.Enabled = !isOriginText;
                this._mouseOverTooltip.SetToolTip(this._button, newText);
            }
        }

        private void ReturnTextToInitial(object sender, EventArgs e)
        {
            this.TryChangeText(this._originBuferText);
        }
    }
}
