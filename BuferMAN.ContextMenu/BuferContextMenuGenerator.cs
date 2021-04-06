using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using ClipboardViewerForm.ClipMenu.Items;
using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.ContextMenu.Properties;
using BuferMAN.View;
using System.Collections.Generic;
using BuferMAN.Infrastructure.Menu;

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

        public IEnumerable<BuferMANMenuItem> GenerateContextMenu(BuferViewModel buferViewModel, Button button, ToolTip mouseOverTooltip, bool isChangeTextAvailable, IBuferSelectionHandler buferSelectionHandler, IBufermanHost bufermanHost)
        {
            var model = new BuferMenuModel(this._clipboardBuferService, buferSelectionHandler, bufermanHost)
            {
                BuferViewModel = buferViewModel,
                Button = button,
                MouseOverTooltip = mouseOverTooltip
            };

            var menuItems = new List<BuferMANMenuItem>();

            model.MarkAsPinnedMenuItem = bufermanHost.CreateMenuItem(Resource.MenuPin, model.TryPinBufer);
            model.MarkAsPinnedMenuItem.ShortCut = Shortcut.CtrlS;
            model.MarkAsPinnedMenuItem.Enabled = !buferViewModel.Pinned;
            menuItems.Add(model.MarkAsPinnedMenuItem);

            var formats = model.BuferViewModel.Clip.GetFormats();
            var formatsCount = formats.Length;

            var formatsMenuItems = new List<BuferMANMenuItem>();

            foreach (var format in formats)
            {
                if (format != ClipboardFormats.CUSTOM_IMAGE_FORMAT && format != ClipboardFormats.FROM_FILE_FORMAT)
                {
                    var particularFormatMenu = bufermanHost.CreateMenuItem(format);

                    var formatData = model.BuferViewModel.Clip.GetData(format);

                    if (formatData is Stream)
                    {
                        particularFormatMenu.Text += " (Stream)";
                    } else if (formatData is string)
                    {
                        particularFormatMenu.Text += " (Text)";
                    }

                    particularFormatMenu.SetOnClickHandler((object sender, EventArgs args) =>
                    {
                        bufermanHost.UserInteraction.ShowPopup(formatData.ToString(), format);
                    });
                    formatsMenuItems.Add(particularFormatMenu);
                }
                else
                {
                    formatsCount -= 1;
                }
            }

            var formatsMenu = bufermanHost.CreateMenuItem(Resource.MenuFormats + $" ({formatsCount})");
            formatsMenu.ShortCut = Shortcut.AltDownArrow;
            
            menuItems.Add(formatsMenu);
            var deleteBuferMenuItem = bufermanHost.CreateMenuItem(Resource.DeleteClipMenuItem);
            model.DeleteMenuItem = new DeleteClipMenuItem(deleteBuferMenuItem, this._clipboardBuferService, model.BuferViewModel, model.Button, bufermanHost);
            menuItems.Add(deleteBuferMenuItem);

            model.PasteMenuItem = bufermanHost.CreateMenuItem(Resource.MenuPaste);

            menuItems.Add(model.PasteMenuItem);
            model.PasteMenuItem.AddMenuItem(bufermanHost.CreateMenuItem(Resource.MenuPasteAsIs + $" {new String('\t', 4)} Enter", (object sender, EventArgs ars) =>
            {
                new KeyboardEmulator().PressEnter();
            }));

            if (formats.Length != 3 || ClipboardFormats.TextFormats.Any(tf => !formats.Contains(tf)))
            {
                var pasteAsTextMenuItem = bufermanHost.CreateMenuItem(Resource.MenuPasteAsText, (object sender, EventArgs args) =>
                {
                    var textDataObject = new DataObject();
                    textDataObject.SetText(buferViewModel.OriginBuferText);

                    var textBuferSelectionHandler = this._buferSelectionHandlerFactory.CreateHandler(textDataObject);
                    textBuferSelectionHandler.DoOnClipSelection(sender, args);
                });
                model.PasteMenuItem.AddMenuItem(pasteAsTextMenuItem);

                pasteAsTextMenuItem.ShortCut = Shortcut.CtrlA;
            }

            model.PasteMenuItem.AddMenuItem(bufermanHost.CreateMenuItem(Resource.MenuCharByChar, (object sender, EventArgs args) =>
            {
                WindowLevelContext.Current.HideWindow();
                new KeyboardEmulator().TypeText((model.Button.Tag as BuferViewModel).OriginBuferText);
            }));

            model.PasteMenuItem.AddSeparator();

            model.PlaceInBuferMenuItem = bufermanHost.CreateMenuItem(Resource.MenuPlaceInBufer, (object sender, EventArgs e) =>
            {
                this._clipboardWrapper.SetDataObject(model.BuferViewModel.Clip);
            });
            model.PlaceInBuferMenuItem.ShortCut = Shortcut.CtrlC;
            model.PasteMenuItem.AddMenuItem(model.PlaceInBuferMenuItem);

            if (isChangeTextAvailable)
            {
                menuItems.Add(bufermanHost.CreateMenuSeparatorItem());

                var returnTextToInitialMenuItem = bufermanHost.CreateMenuItem(Resource.MenuReturn);
                new ReturnToInitialTextMenuItem(returnTextToInitialMenuItem, model.Button, model.MouseOverTooltip, bufermanHost);
                model.ReturnTextToInitialMenuItem = returnTextToInitialMenuItem;
                menuItems.Add(model.ReturnTextToInitialMenuItem);
                var changeTextMenuItem = bufermanHost.CreateMenuItem(Resource.MenuChange);
                var ctmi = new ChangeTextMenuItem(changeTextMenuItem, model.Button, model.MouseOverTooltip, bufermanHost);
                if (!string.IsNullOrWhiteSpace(buferViewModel.Alias))
                {
                    ctmi.TryChangeText(buferViewModel.Alias);
                }
                ctmi.TextChanged += model.ChangeTextMenuItem_TextChanged;
                model.ChangeTextMenuItem = changeTextMenuItem;
                menuItems.Add(model.ChangeTextMenuItem);

                model.AddToFileMenuItem = bufermanHost.CreateMenuItem(Resource.MenuAddToFile);
                model.AddToFileMenuItem.ShortCut = Shortcut.CtrlF;

                if (formats.Contains(ClipboardFormats.FROM_FILE_FORMAT))
                {
                    model.MarkMenuItemAsAddedToFile();
                } else
                {
                    model.AddToFileMenuItem.SetOnClickHandler((object sender, EventArgs args) =>
                    {
                        using (var sw = new StreamWriter(new FileStream(this._settings.DefaultBufersFileName, FileMode.Append, FileAccess.Write)))
                        {
                            sw.WriteLine();
                            sw.WriteLine((model.Button.Tag as BuferViewModel).OriginBuferText);
                        }
                        model.MarkMenuItemAsAddedToFile();
                    });
                }
                menuItems.Add(model.AddToFileMenuItem);

                var loginCredentialsMenuItem = bufermanHost.CreateMenuItem(Resource.CreateCredsMenuItem);
                var clcmi = new CreateLoginCredentialsMenuItem(loginCredentialsMenuItem, model.Button, model.MouseOverTooltip, bufermanHost);
                clcmi.LoginCreated += model.LoginCredentialsMenuItem_LoginCreated;
                model.CreateLoginDataMenuItem = loginCredentialsMenuItem;
                menuItems.Add(model.CreateLoginDataMenuItem);
            }

            menuItems.Add(bufermanHost.CreateMenuSeparatorItem());
            var createdAtMenuItem = bufermanHost.CreateMenuItem(string.Format(Resource.MenuCreatedTime, buferViewModel.CreatedAt));
            createdAtMenuItem.Enabled = false;
            menuItems.Add(createdAtMenuItem);

            return menuItems;
        }

        private class BuferMenuModel
        {
            private readonly IClipboardBuferService _clipboardBuferService;
            private readonly IBuferSelectionHandler _buferSelectionHandler;
            private readonly IBufermanHost _bufermanHost;

            public BuferViewModel BuferViewModel;
            public Button Button;
            public BuferMANMenuItem ReturnTextToInitialMenuItem;
            public BuferMANMenuItem ChangeTextMenuItem;
            public BuferMANMenuItem MarkAsPinnedMenuItem;
            public BuferMANMenuItem CreateLoginDataMenuItem;
            public BuferMANMenuItem AddToFileMenuItem;
            public BuferMANMenuItem PasteMenuItem;
            public BuferMANMenuItem PlaceInBuferMenuItem;
            public DeleteClipMenuItem DeleteMenuItem { get; set; }
            public ToolTip MouseOverTooltip;

            public BuferMenuModel(IClipboardBuferService clipboardBuferService, IBuferSelectionHandler buferSelectionHandler, IBufermanHost bufermanHost)
            {
                this._clipboardBuferService = clipboardBuferService;
                this._buferSelectionHandler = buferSelectionHandler;
                this._bufermanHost = bufermanHost;
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
                    this._bufermanHost.RerenderBufers();

                    if (this.DeleteMenuItem.IsDeferredDeletionActivated())
                    {
                        this.DeleteMenuItem.CancelDeferredBuferDeletion(sender, e);
                    }
                }
            }
        }
    }
}