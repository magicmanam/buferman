using ClipboardBufer;
using ClipboardViewerForm.Window;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

            this._markAsPersistentMenuItem = new MenuItem("Mark as persistent", this.MarkAsPersistent);
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

            formatsMenu.Text = $"Formats ({formatsCount})";

            contextMenu.MenuItems.Add(formatsMenu);
            contextMenu.MenuItems.Add(new MenuItem("Delete", this.DeleteBufer, Shortcut.Del));

            this._pasteMenuItem = new MenuItem($"Paste {new String('\t', 4)} Enter", (object sender, EventArgs ars) =>
            {
                SendKeys.Send("~");
            });
            contextMenu.MenuItems.Add(this._pasteMenuItem);
            
            if (isChangeTextAvailable)
            {
                contextMenu.MenuItems.Add("-");
                contextMenu.MenuItems.Add(new MenuItem("Paste char by char (for console)", (object sender, EventArgs args) => {
                    SendKeys.Flush();
                    this._hidingHandler.HideWindow();
                    foreach (var escapedChar in ClipMenuGenerator._ReplaceSpecialSendKeysCharacters(this._originBuferText))
                    {
                        SendKeys.SendWait(escapedChar);
                    }
                }));

                this._returnTextToInitialMenuItem = new MenuItem("Return text to initial", ReturnTextToInitial) { Enabled = false };
                contextMenu.MenuItems.Add(_returnTextToInitialMenuItem);
                contextMenu.MenuItems.Add(new MenuItem("Change text", this.ChangeText));

                this._addToFileMenuItem = new MenuItem("Add to bufers.txt file", (object sender, EventArgs args) =>
                {
                    using (var sw = new StreamWriter(new FileStream("bufers.txt", FileMode.Append, FileAccess.Write)))
                    {
                        sw.WriteLine(this._originBuferText);
                    }
                    this._addToFileMenuItem.Text = "Added to bufers.txt file";
                    this._addToFileMenuItem.Enabled = false;
                }, Shortcut.CtrlF);
                contextMenu.MenuItems.Add(this._addToFileMenuItem);

                this._createLoginDataMenuItem = new MenuItem("Create login credentials", this._CreateLoginCredentials);
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
            var password = Interaction.InputBox($"Enter a password for login name \"{this._originBuferText}\". TAB between username and password will be inserted automatically as well as Enter after password.",
                   "Create login credentials",
                   null);

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Password can not be empty or whitespaces. Try again! Try better!");
            } else
            {
                var escapedPasswordChars = ClipMenuGenerator._ReplaceSpecialSendKeysCharacters(password);
                var escapedOriginalTextChars = ClipMenuGenerator._ReplaceSpecialSendKeysCharacters(this._originBuferText);
                this._createLoginDataMenuItem.Text = "Login credentials";
                this._pasteMenuItem.Text = $"Login with creds {new String('\t', 2)} Enter";
                this._dataObject.SetData(ClipboardFormats.PASSWORD_FORMAT, password);
                this.TryChangeText($"Creds for {this._button.Text}");

                this._button.Click -= this._buferSelectionHandler.DoOnClipSelection;
                this._button.Click += (object pasteCredsSender, EventArgs args) =>
                {
                    SendKeys.Flush();
                    this._hidingHandler.HideWindow();
                    foreach (var escapedChar in escapedOriginalTextChars)
                    {
                        SendKeys.SendWait(escapedChar);
                    }
                
                    SendKeys.SendWait("{TAB}");
                    foreach (var escapedChar in escapedPasswordChars)
                    {
                        SendKeys.SendWait(escapedChar);
                    }

                    SendKeys.Send("~");
                };

                this._createLoginDataMenuItem.MenuItems.Add(new MenuItem("Paste password with Enter", (object pastePasswordSender, EventArgs args) =>
                {
                    SendKeys.Flush();
                    this._hidingHandler.HideWindow();
                    foreach (var escapedChar in escapedPasswordChars)
                    {
                        SendKeys.SendWait(escapedChar);
                    }
                    SendKeys.Send("~");
                }));
                this._createLoginDataMenuItem.MenuItems.Add(new MenuItem("Paste password", (object pastePasswordSender, EventArgs args) =>
                {
                    SendKeys.Flush();
                    this._hidingHandler.HideWindow();
                    foreach (var escapedChar in escapedPasswordChars)
                    {
                        SendKeys.SendWait(escapedChar);
                    }
                }));
                this._createLoginDataMenuItem.MenuItems.Add(new MenuItem("Paste username", (object pasteUsernameSender, EventArgs args) =>
                {
                    SendKeys.Flush();
                    this._hidingHandler.HideWindow();
                    foreach (var escapedChar in escapedOriginalTextChars)
                    {
                        SendKeys.SendWait(escapedChar);
                    }
                }));
                MarkAsPersistent(sender, e);
            }
        }

        private static IEnumerable<string> _ReplaceSpecialSendKeysCharacters(string password)
        {
            const string specialSendKeysChars = "+^%[]{}";

            return password.ToCharArray().Select(c => specialSendKeysChars.Contains(c) ? $"{{{c}}}" : c.ToString());
        }

        private void ChangeText(object sender, EventArgs e)
        {
            var newText = Interaction.InputBox($"Enter a new text for this bufer. It can be useful to hide copied passwords or alias some enourmous text. Primary button value was \"{this._originBuferText}\". If you need save login/password pair, just use 'Create login credentials menu'.",
                   "Change bufer's text",
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
                    MessageBox.Show("Bufer alias was returned to its primary value");

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
