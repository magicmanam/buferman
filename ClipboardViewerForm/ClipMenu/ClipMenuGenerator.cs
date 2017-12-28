using ClipboardBufer;
using ClipboardViewerForm.Window;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private IDataObject _dataObject;
        private Button _button;
        private MenuItem _returnTextToInitialMenuItem;
        private MenuItem _markAsPersistentMenuItem;
        private String _originBuferText;
        private string _tooltipText;
        private ToolTip _mouseOverTooltip;

        public ClipMenuGenerator(IClipboardBuferService clipboardBuferService, IRenderingHandler renderingHandler)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._renderingHandler = renderingHandler;
        }

        public ContextMenu GenerateContextMenu(IDataObject dataObject, Button button, String originBuferText, string tooltipText, ToolTip mouseOverTooltip, bool isChangeTextAvailable)
        {
            this._dataObject = dataObject;
            this._button = button;
            this._originBuferText = originBuferText;
            this._tooltipText = tooltipText;
            this._mouseOverTooltip = mouseOverTooltip;

            var contextMenu = new ContextMenu();
            var formats = this._dataObject.GetFormats();
            var formatsMenu = new MenuItem($"Formats ({formats.Length})");
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
            }

            contextMenu.MenuItems.Add(formatsMenu);
            contextMenu.MenuItems.Add(new MenuItem("Delete", this.DeleteBufer, Shortcut.Del));
            if (isChangeTextAvailable)
            {
                contextMenu.MenuItems.Add(new MenuItem("Change text", this.ChangeText));
                _returnTextToInitialMenuItem = new MenuItem("Return text to initial", ReturnTextToInitial) { Enabled = false };
                contextMenu.MenuItems.Add(_returnTextToInitialMenuItem);
            }
            contextMenu.MenuItems.Add(new MenuItem("Paste", (object sender, EventArgs ars) =>
            {
                SendKeys.Send("~");
            }));
            this._markAsPersistentMenuItem = new MenuItem("Mark as persistent", this.MarkAsPersistent);
            contextMenu.MenuItems.Add(this._markAsPersistentMenuItem);

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

        private void ChangeText(object sender, EventArgs e)
        {
            var newText = Microsoft.VisualBasic.Interaction.InputBox($"Enter a new text for this bufer. It can be useful to hide copied passwords or alias some enourmous text. Primary button value was \"{this._originBuferText}\".",
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
