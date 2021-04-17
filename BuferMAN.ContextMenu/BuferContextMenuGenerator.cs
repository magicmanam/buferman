using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using ClipboardViewerForm.ClipMenu.Items;
using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using magicmanam.Windows;
using BuferMAN.View;
using System.Collections.Generic;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Plugins;
using System.Diagnostics;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;

namespace BuferMAN.ContextMenu
{
    public class BuferContextMenuGenerator : IBuferContextMenuGenerator
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IBuferSelectionHandlerFactory _buferSelectionHandlerFactory;
        private readonly IProgramSettings _settings;
        private readonly IClipboardWrapper _clipboardWrapper;
        private readonly IEnumerable<IBufermanPlugin> _plugins;
        private readonly IBufersStorageFactory _bufersStorageFactory;
        private readonly IUserFileSelector _userFileSelector;

        public BuferContextMenuGenerator(IClipboardBuferService clipboardBuferService, IBuferSelectionHandlerFactory buferSelectionHandlerFactory, IProgramSettings settings, IClipboardWrapper clipboardWrapper, IEnumerable<IBufermanPlugin> plugins,
            IBufersStorageFactory bufersStorageFactory, IUserFileSelector userFileSelector)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._buferSelectionHandlerFactory = buferSelectionHandlerFactory;
            this._settings = settings;
            this._clipboardWrapper = clipboardWrapper;
            this._plugins = plugins;
            this._bufersStorageFactory = bufersStorageFactory;
            this._userFileSelector = userFileSelector;
        }

        public IEnumerable<BufermanMenuItem> GenerateContextMenu(BuferViewModel buferViewModel, Button button, ToolTip mouseOverTooltip, bool isChangeTextAvailable, IBuferSelectionHandler buferSelectionHandler, IBufermanHost bufermanHost)
        {
            var model = new BuferContextMenuModelWrapper(this._clipboardBuferService, buferSelectionHandler, bufermanHost)
            {
                BuferViewModel = buferViewModel,
                Button = button,
                MouseOverTooltip = mouseOverTooltip
            };

            var menuItems = new List<BufermanMenuItem>();

            model.MarkAsPinnedMenuItem = bufermanHost.CreateMenuItem(buferViewModel.Pinned ? Resource.MenuUnpin : Resource.MenuPin, model.TryTogglePinBufer);
            model.MarkAsPinnedMenuItem.ShortCut = Shortcut.CtrlS;
            menuItems.Add(model.MarkAsPinnedMenuItem);

            var formats = model.BuferViewModel.Clip.GetFormats();
            var formatsMenuItems = new List<BufermanMenuItem>();

            foreach (var format in formats)
            {
                if (format != ClipboardFormats.CUSTOM_IMAGE_FORMAT && format != ClipboardFormats.FROM_FILE_FORMAT)
                {
                    var particularFormatMenu = bufermanHost.CreateMenuItem(format);

                    var formatData = model.BuferViewModel.Clip.GetData(format);

                    if (formatData is Stream)
                    {
                        particularFormatMenu.Text += " (Stream)";
                    }
                    else if (formatData is string)
                    {
                        particularFormatMenu.Text += " (Text)";
                    }

                    if (formatData == null)
                    {
                        particularFormatMenu.Enabled = false;
                    }
                    else
                    {
                        particularFormatMenu.AddOnClickHandler((object sender, EventArgs args) =>
                        {
                            bufermanHost.UserInteraction.ShowPopup(formatData.ToString(), format);
                        });
                    }
                    formatsMenuItems.Add(particularFormatMenu);
                }
            }

            var formatsMenuItem = bufermanHost.CreateMenuItem(Resource.MenuFormats + $" ({formatsMenuItems.Count})");
            foreach (var formatMenuItem in formatsMenuItems)
            {
                formatsMenuItem.AddMenuItem(formatMenuItem);
            }

            menuItems.Add(formatsMenuItem);
            var deleteBuferMenuItem = bufermanHost.CreateMenuItem(Resource.DeleteBuferMenuItem);
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

                if (Uri.TryCreate(buferViewModel.OriginBuferText, UriKind.Absolute, out var uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    var openInBrowserMenuItem = bufermanHost.CreateMenuItem(Resource.MenuOpenInBrowser, (object sender, EventArgs e) =>
                                        Process.Start(buferViewModel.OriginBuferText));
                    openInBrowserMenuItem.ShortCut = Shortcut.CtrlB;
                    menuItems.Add(openInBrowserMenuItem);
                }

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

                if (formats.Contains(ClipboardFormats.FROM_FILE_FORMAT))
                {
                    model.MarkMenuItemAsAddedToFile();
                } else
                {
                    var addToDefaultFileMenuItem = bufermanHost.CreateMenuItem(Resource.MenuAddToDefaultFile, (object sender, EventArgs args) =>
                    {
                        var bufersStorage = this._bufersStorageFactory.CreateStorageByFileExtension(this._settings.DefaultBufersFileName);

                        var buferItem = this._GetBuferItemFromModel(model);

                        bufersStorage.SaveBufer(buferItem);

                        model.MarkMenuItemAsAddedToFile();
                    });

                    addToDefaultFileMenuItem.ShortCut = Shortcut.CtrlF;
                    model.AddToFileMenuItem.AddMenuItem(addToDefaultFileMenuItem);

                    var addToFileMenuItem = bufermanHost.CreateMenuItem(Resource.MenuAddToSelectedFile, (object sender, EventArgs args) =>
                    {
                        var buferItem = this._GetBuferItemFromModel(model);

                        this._userFileSelector.TrySelectBufersStorage(storage => storage.SaveBufer(buferItem));
                    });

                    model.AddToFileMenuItem.AddSeparator();
                    model.AddToFileMenuItem.AddMenuItem(addToFileMenuItem);


                }
                menuItems.Add(model.AddToFileMenuItem);

                var loginCredentialsMenuItem = bufermanHost.CreateMenuItem(Resource.CreateCredsMenuItem);
                var clcmi = new CreateLoginCredentialsMenuItem(loginCredentialsMenuItem, model.Button, model.MouseOverTooltip, bufermanHost);
                clcmi.LoginCreated += model.LoginCredentialsMenuItem_LoginCreated;
                model.CreateLoginDataMenuItem = loginCredentialsMenuItem;
                menuItems.Add(model.CreateLoginDataMenuItem);
            }

            foreach (var plugin in this._plugins) if (plugin.Enabled)
                {
                    plugin.UpdateBuferContextMenu(model);
                    var pluginMenuItem = plugin.CreateBuferContextMenuItem();
                    if (pluginMenuItem != null)
                    {
                        menuItems.Add(pluginMenuItem);
                    }
                }

            menuItems.Add(bufermanHost.CreateMenuSeparatorItem());
            var createdAtMenuItem = bufermanHost.CreateMenuItem(string.Format(Resource.MenuCopyingTime, buferViewModel.CreatedAt));
            createdAtMenuItem.Enabled = false;
            menuItems.Add(createdAtMenuItem);

            return menuItems;
        }

        private BuferItem _GetBuferItemFromModel(BuferContextMenuModelWrapper model)
        {
            var buferModel = model.Button.Tag as BuferViewModel;
            var buferItem = new BuferItem()
            {
                Pinned = buferModel.Pinned,
                Alias = buferModel.Alias,
                Formats = buferModel.Clip
                                        .GetFormats()
                                        .ToDictionary(
                                                 f => f,
                                                 f => buferModel.Clip.GetData(f))
            };
            return buferItem;
        }
    }
}