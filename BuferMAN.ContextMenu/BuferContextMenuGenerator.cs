using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using ClipboardViewerForm.ClipMenu.Items;
using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using magicmanam.Windows;
using System.Collections.Generic;
using BuferMAN.Infrastructure.Menu;
using BuferMAN.Infrastructure.Plugins;
using System.Diagnostics;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Models;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Infrastructure.Files;

namespace BuferMAN.ContextMenu
{
    internal class BuferContextMenuGenerator : IBuferContextMenuGenerator
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IBuferSelectionHandlerFactory _buferSelectionHandlerFactory;
        private readonly IProgramSettingsGetter _settings;
        private readonly IClipboardWrapper _clipboardWrapper;
        private readonly IEnumerable<IBufermanPlugin> _plugins;
        private readonly IBufersStorageFactory _bufersStorageFactory;
        private readonly IUserFileSelector _userFileSelector;
        private readonly IMapper _mapper;

        public BuferContextMenuGenerator(IClipboardBuferService clipboardBuferService,
            IBuferSelectionHandlerFactory buferSelectionHandlerFactory,
            IProgramSettingsGetter settings,
            IClipboardWrapper clipboardWrapper,
            IEnumerable<IBufermanPlugin> plugins,
            IBufersStorageFactory bufersStorageFactory, 
            IUserFileSelector userFileSelector,
            IMapper mapper)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._buferSelectionHandlerFactory = buferSelectionHandlerFactory;
            this._settings = settings;
            this._clipboardWrapper = clipboardWrapper;
            this._plugins = plugins;
            this._bufersStorageFactory = bufersStorageFactory;
            this._userFileSelector = userFileSelector;
            this._mapper = mapper;
        }

        public IEnumerable<BufermanMenuItem> GenerateContextMenuItems(IBufer bufer,
            bool isChangeTextAvailable,
            IBuferSelectionHandler buferSelectionHandler,
            IBufermanHost bufermanHost,
            IBuferTypeMenuGenerator buferTypeMenuGenerator)
        {
            var model = new BuferContextMenuModelWrapper(this._clipboardBuferService, buferSelectionHandler, bufermanHost)
            {
                Bufer = bufer
            };
            var buferViewModel = bufer.ViewModel;

            IList<BufermanMenuItem> menuItems = new List<BufermanMenuItem>();

            model.MarkAsPinnedMenuItem = bufermanHost.CreateMenuItem(bufer.ViewModel.Pinned ? Resource.MenuUnpin : Resource.MenuPin, model.TryTogglePinBufer);
            model.MarkAsPinnedMenuItem.ShortCut = Shortcut.CtrlS;
            menuItems.Add(model.MarkAsPinnedMenuItem);

            var formats = model.Bufer.ViewModel.Clip.GetFormats();
            var formatsMenuItems = new List<BufermanMenuItem>();

            foreach (var format in formats)
            {
                if (format != ClipboardFormats.CUSTOM_IMAGE_FORMAT && format != ClipboardFormats.FROM_FILE_FORMAT)
                {
                    var particularFormatMenu = bufermanHost.CreateMenuItem(format);

                    var formatData = model.Bufer.ViewModel.Clip.GetData(format);

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
                            int maxBuferLength = this._settings.MaxBuferPresentationLength;
                            var data = formatData.ToString();
                            if (data.Length > maxBuferLength)
                            {
                                data = data.Substring(0, maxBuferLength - 300) + Environment.NewLine + Environment.NewLine + "...";
                            }
                            var title = $"{format}";// TODO (s) maybe change this title to be more descriptive?
                            bufermanHost.UserInteraction.ShowPopup(data, title);
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
            model.DeleteMenuItem = new DeleteClipMenuItem(deleteBuferMenuItem, this._clipboardBuferService, model.Bufer, bufermanHost);
            menuItems.Add(deleteBuferMenuItem);

            model.PasteMenuItem = bufermanHost.CreateMenuItem(Resource.MenuPaste);

            menuItems.Add(model.PasteMenuItem);

            model.PlaceInBuferMenuItem = bufermanHost.CreateMenuItem(Resource.MenuPlaceInBufer, (object sender, EventArgs e) =>
            {
                this._clipboardWrapper.SetDataObject(model.Bufer.ViewModel.Clip);
            });
            model.PlaceInBuferMenuItem.ShortCut = Shortcut.CtrlC;

            if (isChangeTextAvailable)
            {
                model.PasteMenuItem.AddMenuItem(bufermanHost.CreateMenuItem(Resource.MenuPasteAsIs + $" {new String('\t', 4)} Enter", (object sender, EventArgs ars) =>
                {
                    new KeyboardEmulator().PressEnter();
                }));

                if (formats.Length != 3 || ClipboardFormats.TextFormats.Any(tf => !formats.Contains(tf)))
                {
                    var pasteAsTextMenuItem = bufermanHost.CreateMenuItem(Resource.MenuPasteAsText, (object sender, EventArgs args) =>
                    {
                        var textDataObject = new DataObject();
                        textDataObject.SetText(buferViewModel.OriginBuferTitle);

                        var textBuferSelectionHandler = this._buferSelectionHandlerFactory.CreateHandler(textDataObject, bufermanHost);
                        textBuferSelectionHandler.DoOnClipSelection(sender, args);
                    });
                    model.PasteMenuItem.AddMenuItem(pasteAsTextMenuItem);

                    pasteAsTextMenuItem.ShortCut = Shortcut.CtrlA;
                }

                model.PasteMenuItem.AddMenuItem(bufermanHost.CreateMenuItem(Resource.MenuCharByChar, (object sender, EventArgs args) =>
                {
                    bufermanHost.HideWindow();
                    new KeyboardEmulator().TypeText(model.Bufer.ViewModel.OriginBuferTitle);
                }));

                menuItems.Add(bufermanHost.CreateMenuSeparatorItem());

                if (Uri.TryCreate(buferViewModel.OriginBuferTitle, UriKind.Absolute, out var uriResult)
                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    var openInBrowserMenuItem = bufermanHost.CreateMenuItem(Resource.MenuOpenInBrowser, (object sender, EventArgs e) =>
                                        Process.Start(buferViewModel.OriginBuferTitle));
                    openInBrowserMenuItem.ShortCut = Shortcut.CtrlB;
                    menuItems.Add(openInBrowserMenuItem);
                }

                var returnTextToInitialMenuItem = bufermanHost.CreateMenuItem(Resource.MenuReturn);
                new ReturnToInitialTextMenuItem(returnTextToInitialMenuItem, model.Bufer, bufermanHost);
                model.ReturnTextToInitialMenuItem = returnTextToInitialMenuItem;
                var changeTextMenuItem = bufermanHost.CreateMenuItem(Resource.MenuChange);
                var ctmi = new ChangeTextMenuItem(changeTextMenuItem, model.Bufer, bufermanHost);
                if (!string.IsNullOrWhiteSpace(buferViewModel.Alias))
                {
                    ctmi.TryChangeText(buferViewModel.Alias);
                }
                ctmi.TextChanged += model.ChangeTextMenuItem_TextChanged;
                model.ChangeTextMenuItem = changeTextMenuItem;
                menuItems.Add(model.ChangeTextMenuItem);
                menuItems.Add(model.ReturnTextToInitialMenuItem);

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
                var clcmi = new CreateLoginCredentialsMenuItem(loginCredentialsMenuItem, model.Bufer, bufermanHost);
                clcmi.LoginCreated += model.LoginCredentialsMenuItem_LoginCreated;
                model.CreateLoginDataMenuItem = loginCredentialsMenuItem;
                menuItems.Add(model.CreateLoginDataMenuItem);

                model.PasteMenuItem.AddSeparator();

                model.PasteMenuItem.AddMenuItem(model.PlaceInBuferMenuItem);
            }
            else
            {
                model.PasteMenuItem.Text += $" {new String('\t', 4)} Enter";
                model.PasteMenuItem.AddOnClickHandler((object sender, EventArgs ars) =>
                {
                    new KeyboardEmulator().PressEnter();
                });

                menuItems.Add(model.PlaceInBuferMenuItem);
            }

            if (buferTypeMenuGenerator != null)
            {
                menuItems = buferTypeMenuGenerator.Generate(menuItems, bufermanHost);
            }

            foreach (var plugin in this._plugins) if (plugin.Available && plugin.Enabled)
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
            var buferViewModel = model.Bufer.ViewModel;

            return this._mapper.Map(buferViewModel);
        }
    }
}