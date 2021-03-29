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
using BuferMAN.View;

namespace BuferMAN.ContextMenu
{
    public class BuferContextMenuGenerator : IBuferContextMenuGenerator
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IBuferSelectionHandlerFactory _buferSelectionHandlerFactory;
        private readonly IProgramSettings _settings;
        private readonly IClipboardWrapper _clipboardWrapper;

        public BuferContextMenuGenerator(IClipboardBuferService clipboardBuferService, IBuferSelectionHandlerFactory buferSelectionHandlerFactory, IProgramSettings settings, IClipboardWrapper clipboardWrapper)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._buferSelectionHandlerFactory = buferSelectionHandlerFactory;
            this._settings = settings;
            this._clipboardWrapper = clipboardWrapper;
        }

        public SystemWindowsFormsContextMenu GenerateContextMenu(BuferViewModel buferViewModel, Button button, ToolTip mouseOverTooltip, bool isChangeTextAvailable, IBuferSelectionHandler buferSelectionHandler)
        {
            var model = new BuferMenuModel(this._clipboardBuferService, buferSelectionHandler);
            model.BuferViewModel = buferViewModel;
            model.Button = button;
            model.MouseOverTooltip = mouseOverTooltip;

            var contextMenu = new SystemWindowsFormsContextMenu();

            model.MarkAsPinnedMenuItem = new MakePinnedMenuItem();
            model.MarkAsPinnedMenuItem.Click += model.TryPinBufer;
            model.MarkAsPinnedMenuItem.Enabled = !buferViewModel.Pinned;
            contextMenu.MenuItems.Add(model.MarkAsPinnedMenuItem);

            var formats = model.BuferViewModel.Clip.GetFormats();
            var formatsMenu = new MenuItem();
            var formatsCount = formats.Length;
            
            formatsMenu.Shortcut = Shortcut.AltDownArrow;
            foreach (var format in formats)
            {
                if (format != ClipboardFormats.CUSTOM_IMAGE_FORMAT && format != ClipboardFormats.FROM_FILE_FORMAT)
                {
                    var particularFormatMenu = new MenuItem(format);
                    var formatData = model.BuferViewModel.Clip.GetData(format);

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
            contextMenu.MenuItems.Add(new DeleteClipMenuItem(this._clipboardBuferService, model.BuferViewModel, button));

            model.PasteMenuItem = new MenuItem(Resource.MenuPaste);

            contextMenu.MenuItems.Add(model.PasteMenuItem);
            model.PasteMenuItem.MenuItems.Add(new MenuItem(Resource.MenuPasteAsIs + $" {new String('\t', 4)} Enter", (object sender, EventArgs ars) =>
            {
                new KeyboardEmulator().PressEnter();
            }));

            if (formats.Length != 3 || ClipboardFormats.TextFormats.Any(tf => !formats.Contains(tf)))
            {
                model.PasteMenuItem.MenuItems.Add(new MenuItem(Resource.MenuPasteAsText, (object sender, EventArgs args) =>
                {
                    var textDataObject = new DataObject();
                    textDataObject.SetText(buferViewModel.OriginBuferText);

                    var textBuferSelectionHandler = this._buferSelectionHandlerFactory.CreateHandler(textDataObject);
                    textBuferSelectionHandler.DoOnClipSelection(sender, args);
                }, Shortcut.CtrlA));
            }

            model.PasteMenuItem.MenuItems.Add(new MenuItem(Resource.MenuCharByChar, (object sender, EventArgs args) =>
            {
                WindowLevelContext.Current.HideWindow();
                new KeyboardEmulator().TypeText((model.Button.Tag as BuferViewModel).OriginBuferText);
            }));

            model.PasteMenuItem.MenuItems.AddSeparator();

            model.PlaceInBuferMenuItem = new PlaceInBuferMenuItem(this._clipboardWrapper, model.BuferViewModel.Clip);
            model.PasteMenuItem.MenuItems.Add(model.PlaceInBuferMenuItem);

            if (isChangeTextAvailable)
            {
                contextMenu.MenuItems.AddSeparator();

                model.ReturnTextToInitialMenuItem = new ReturnToInitialTextMenuItem(model.Button, model.MouseOverTooltip);
                contextMenu.MenuItems.Add(model.ReturnTextToInitialMenuItem);
                var changeTextMenuItem = new ChangeTextMenuItem(model.Button, model.MouseOverTooltip);
                if (!string.IsNullOrWhiteSpace(buferViewModel.Alias))
                {
                    changeTextMenuItem.TryChangeText(buferViewModel.Alias);
                }
                changeTextMenuItem.TextChanged += model.ChangeTextMenuItem_TextChanged;
                model.ChangeTextMenuItem = changeTextMenuItem;
                contextMenu.MenuItems.Add(model.ChangeTextMenuItem);

                model.AddToFileMenuItem = new MenuItem(Resource.MenuAddToFile);
                model.AddToFileMenuItem.Shortcut = Shortcut.CtrlF;

                if (formats.Contains(ClipboardFormats.FROM_FILE_FORMAT))
                {
                    model.MarkMenuItemAsAddedToFile();
                } else
                {
                    model.AddToFileMenuItem.Click += (object sender, EventArgs args) =>
                    {
                        using (var sw = new StreamWriter(new FileStream(this._settings.DefaultBufersFileName, FileMode.Append, FileAccess.Write)))
                        {
                            sw.WriteLine();
                            sw.WriteLine((model.Button.Tag as BuferViewModel).OriginBuferText);
                        }
                        model.MarkMenuItemAsAddedToFile();
                    };
                }
                contextMenu.MenuItems.Add(model.AddToFileMenuItem);

                var loginCredentialsMenuItem = new CreateLoginCredentialsMenuItem(model.Button, model.MouseOverTooltip);
                loginCredentialsMenuItem.LoginCreated += model.LoginCredentialsMenuItem_LoginCreated;
                model.CreateLoginDataMenuItem = loginCredentialsMenuItem;
                contextMenu.MenuItems.Add(model.CreateLoginDataMenuItem);
            }

            contextMenu.MenuItems.AddSeparator();
            contextMenu.MenuItems.Add(new MenuItem(string.Format(Resource.MenuCreatedTime, buferViewModel.CreatedAt)));

            return contextMenu;
        }

        private class BuferMenuModel
        {
            private readonly IClipboardBuferService _clipboardBuferService;
            private readonly IBuferSelectionHandler _buferSelectionHandler;

            public BuferViewModel BuferViewModel;
            public Button Button;
            public MenuItem ReturnTextToInitialMenuItem;
            public MenuItem ChangeTextMenuItem;
            public MenuItem MarkAsPinnedMenuItem;
            public MenuItem CreateLoginDataMenuItem;
            public MenuItem AddToFileMenuItem;
            public MenuItem PasteMenuItem;
            public MenuItem PlaceInBuferMenuItem;
            public ToolTip MouseOverTooltip;

            public BuferMenuModel(IClipboardBuferService clipboardBuferService, IBuferSelectionHandler buferSelectionHandler)
            {
                this._clipboardBuferService = clipboardBuferService;
                this._buferSelectionHandler = buferSelectionHandler;
            }

            public void ChangeTextMenuItem_TextChanged(object sender, TextChangedEventArgs e)
            {
                this.ReturnTextToInitialMenuItem.Enabled = !e.IsOriginText;
            }

            public void MarkMenuItemAsAddedToFile()
            {
                this.AddToFileMenuItem.Text = Resource.MenuAddedToFile;
                this.AddToFileMenuItem.Enabled = false;
            }

            public void LoginCredentialsMenuItem_LoginCreated(object sender, CreateLoginCredentialsEventArgs e)
            {
                this.PasteMenuItem.Text = $"{Resource.LoginWith} {new String('\t', 2)} Enter";

                this.ReturnTextToInitialMenuItem.Enabled = false;
                this.PlaceInBuferMenuItem.Enabled = false;
                this.ChangeTextMenuItem.Enabled = false;
                this.BuferViewModel.Clip.SetData(ClipboardFormats.PASSWORD_FORMAT, e.Password);
                if (this.MarkAsPinnedMenuItem.Enabled)
                {
                    this.TryPinBufer(sender, e);
                }
                
                this.Button.Click -= this._buferSelectionHandler.DoOnClipSelection;
            }

            public void TryPinBufer(object sender, EventArgs e)
            {
                if (this._clipboardBuferService.TryPinBufer(this.BuferViewModel.ViewId))
                {
                    this.BuferViewModel.Pinned = true;
                    this.MarkAsPinnedMenuItem.Enabled = false;
                    WindowLevelContext.Current.RerenderBufers();
                }
            }
        }
    }
}
