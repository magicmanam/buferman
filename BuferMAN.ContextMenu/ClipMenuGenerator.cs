using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using ClipboardViewerForm.ClipMenu.Items;
using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using SystemWindowsFormsContextMenu = System.Windows.Forms.ContextMenu;
using magicmanam.Windows;
using BuferMAN.Menu;
using BuferMAN.ContextMenu.Properties;
using System.Drawing;
using BuferMAN.View;

namespace BuferMAN.ContextMenu
{
    public class ClipMenuGenerator : IClipMenuGenerator
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IBuferSelectionHandler _buferSelectionHandler;
        private readonly IProgramSettings _settings;
        private BuferViewModel _buferViewModel;
        private Button _button;
        private MenuItem _returnTextToInitialMenuItem;
        private MenuItem _changeTextMenuItem;
        private MenuItem _markAsPersistentMenuItem;
        private MenuItem _createLoginDataMenuItem;
        private MenuItem _addToFileMenuItem;
        private MenuItem _pasteMenuItem;
        private MenuItem _placeInBuferMenuItem;
        private String _originBuferText;
        private ToolTip _mouseOverTooltip;
        private readonly IClipboardWrapper _clipboardWrapper;

        public ClipMenuGenerator(IClipboardBuferService clipboardBuferService, BuferSelectionHandler buferSelectionHandler, IProgramSettings settings, IClipboardWrapper clipboardWrapper)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._buferSelectionHandler = buferSelectionHandler;
            this._settings = settings;
            this._clipboardWrapper = clipboardWrapper;
        }

        public SystemWindowsFormsContextMenu GenerateContextMenu(BuferViewModel buferViewModel, Button button, ToolTip mouseOverTooltip, bool isChangeTextAvailable)
        {
            this._buferViewModel = buferViewModel;
            this._button = button;
            this._originBuferText = button.Text;
            this._mouseOverTooltip = mouseOverTooltip;

            var contextMenu = new SystemWindowsFormsContextMenu();

            this._markAsPersistentMenuItem = new MakePersistentMenuItem();
            this._markAsPersistentMenuItem.Click += this._TryMarkClipAsPersistent;
            this._markAsPersistentMenuItem.Enabled = !buferViewModel.Persistent;
            contextMenu.MenuItems.Add(this._markAsPersistentMenuItem);
            this._placeInBuferMenuItem = new PlaceInBuferMenuItem(this._clipboardWrapper, this._buferViewModel.Clip);
            contextMenu.MenuItems.Add(this._placeInBuferMenuItem);

            var formats = this._buferViewModel.Clip.GetFormats();
            var formatsMenu = new MenuItem();
            var formatsCount = formats.Length;
            
            formatsMenu.Shortcut = Shortcut.AltDownArrow;
            foreach (var format in formats)
            {
                if (format != ClipboardFormats.CUSTOM_IMAGE_FORMAT && format != ClipboardFormats.FROM_FILE_FORMAT)
                {
                    var particularFormatMenu = new MenuItem(format);
                    var formatData = this._buferViewModel.Clip.GetData(format);

                    if (formatData is Stream)
                    {
                        particularFormatMenu.Text += " (Stream)";
                    } else if (formatData is string)
                    {
                        particularFormatMenu.Text += " (Text)";
                    }

                    particularFormatMenu.Click += (object sender, EventArgs args) =>
                    {
                        MessageBox.Show(formatData.ToString(), format);
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
            contextMenu.MenuItems.Add(new DeleteClipMenuItem(this._clipboardBuferService, this._buferViewModel.Clip, button));

            this._pasteMenuItem = new MenuItem(Resource.MenuPaste + $" {new String('\t', 4)} Enter", (object sender, EventArgs ars) =>
            {
                new KeyboardEmulator().PressEnter();
            });
            contextMenu.MenuItems.Add(this._pasteMenuItem);
            
            if (isChangeTextAvailable)
            {
                contextMenu.MenuItems.AddSeparator();
                contextMenu.MenuItems.Add(new MenuItem(Resource.MenuCharByChar, (object sender, EventArgs args) =>
                {
                    WindowLevelContext.Current.HideWindow();
                    new KeyboardEmulator().TypeText(this._originBuferText);
                }));

                this._returnTextToInitialMenuItem = new ReturnToInitialTextMenuItem(this._button, this._originBuferText, this._mouseOverTooltip);
                contextMenu.MenuItems.Add(this._returnTextToInitialMenuItem);
                var changeTextMenuItem = new ChangeTextMenuItem(this._button, this._originBuferText, this._mouseOverTooltip);
                if (!string.IsNullOrWhiteSpace(buferViewModel.Alias))
                {
                    changeTextMenuItem.TryChangeText(buferViewModel.Alias);
                }
                changeTextMenuItem.TextChanged += this._ChangeTextMenuItem_TextChanged;
                this._changeTextMenuItem = changeTextMenuItem;
                contextMenu.MenuItems.Add(this._changeTextMenuItem);

                this._addToFileMenuItem = new MenuItem(Resource.MenuAddToFile);
                this._addToFileMenuItem.Shortcut = Shortcut.CtrlF;

                if (formats.Contains(ClipboardFormats.FROM_FILE_FORMAT))
                {
                    this._MarkMenuItemAsAddedToFile();
                } else
                {
                    this._addToFileMenuItem.Click += (object sender, EventArgs args) =>
                    {
                        using (var sw = new StreamWriter(new FileStream(this._settings.DefaultBufersFileName, FileMode.Append, FileAccess.Write)))
                        {
                            sw.WriteLine();
                            sw.WriteLine(this._originBuferText);
                        }
                        this._MarkMenuItemAsAddedToFile();
                    };
                }
                contextMenu.MenuItems.Add(this._addToFileMenuItem);

                var loginCredentialsMenuItem = new CreateLoginCredentialsMenuItem(this._button, this._originBuferText, this._mouseOverTooltip);
                loginCredentialsMenuItem.LoginCreated += this._LoginCredentialsMenuItem_LoginCreated;
                this._createLoginDataMenuItem = loginCredentialsMenuItem;
                contextMenu.MenuItems.Add(this._createLoginDataMenuItem);
            }

            contextMenu.MenuItems.AddSeparator();
            contextMenu.MenuItems.Add(new MenuItem(string.Format(Resource.MenuCreatedTime, buferViewModel.CreatedAt)));

            return contextMenu;
        }

        private void _MarkMenuItemAsAddedToFile()
        {
            this._addToFileMenuItem.Text = Resource.MenuAddedToFile;
            this._addToFileMenuItem.Enabled = false;
        }

        private void _LoginCredentialsMenuItem_LoginCreated(object sender, CreateLoginCredentialsEventArgs e)
        {
            this._pasteMenuItem.Text = $"{Resource.LoginWith} {new String('\t', 2)} Enter";

            this._returnTextToInitialMenuItem.Enabled = false;
            this._placeInBuferMenuItem.Enabled = false;
            this._changeTextMenuItem.Enabled = false;
            this._buferViewModel.Clip.SetData(ClipboardFormats.PASSWORD_FORMAT, e.Password);
            if (this._markAsPersistentMenuItem.Enabled)
            {
                this._TryMarkClipAsPersistent(sender, e);
            }
            this._button.Click -= this._buferSelectionHandler.DoOnClipSelection;
        }

        private void _ChangeTextMenuItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            this._returnTextToInitialMenuItem.Enabled = !e.IsOriginText;
        }

        private void _TryMarkClipAsPersistent(object sender, EventArgs e)
        {
            if (this._clipboardBuferService.TryMarkClipAsPersistent(this._buferViewModel.Clip))
            {
                this._buferViewModel.Persistent = true;
                this._markAsPersistentMenuItem.Enabled = false;
                WindowLevelContext.Current.RerenderBufers();
            }
        }
    }
}
