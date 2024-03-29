﻿using BuferMAN.Clipboard;
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
using BuferMAN.Infrastructure.Storage;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Infrastructure.Files;

namespace BuferMAN.ContextMenu
{
    internal class BuferContextMenuGenerator : IBuferContextMenuGenerator
    {
        private readonly IBuferSelectionHandlerFactory _buferSelectionHandlerFactory;
        private readonly IProgramSettingsGetter _settings;
        private readonly IClipboardWrapper _clipboardWrapper;
        private readonly IBufersStorageFactory _bufersStorageFactory;
        private readonly IUserFileSelector _userFileSelector;
        private readonly IFileStorage _fileStorage;

        public BuferContextMenuGenerator(IBuferSelectionHandlerFactory buferSelectionHandlerFactory,
            IProgramSettingsGetter settings,
            IClipboardWrapper clipboardWrapper,
            IBufersStorageFactory bufersStorageFactory, 
            IUserFileSelector userFileSelector,
            IFileStorage fileStorage)
        {
            this._buferSelectionHandlerFactory = buferSelectionHandlerFactory;
            this._settings = settings;
            this._clipboardWrapper = clipboardWrapper;
            this._bufersStorageFactory = bufersStorageFactory;
            this._userFileSelector = userFileSelector;
            this._fileStorage = fileStorage;
        }

        public IEnumerable<BufermanMenuItem> GenerateContextMenuItems(BuferContextMenuState buferContextMenuState,
            IBufermanHost bufermanHost,
            IBuferTypeMenuGenerator buferTypeMenuGenerator)
        {
            var bufer = buferContextMenuState.Bufer;
            var buferViewModel = bufer.ViewModel;

            IList<BufermanMenuItem> menuItems = new List<BufermanMenuItem>();

            buferContextMenuState.MarkAsPinnedMenuItem.ShortCut = Shortcut.CtrlS;
            menuItems.Add(buferContextMenuState.MarkAsPinnedMenuItem);

            var formats = buferContextMenuState.Bufer.ViewModel.Clip.GetFormats();
            var formatsMenuItems = new List<BufermanMenuItem>();

            foreach (var format in formats)
            {
                if (format != ClipboardFormats.CUSTOM_IMAGE_FORMAT && format != ClipboardFormats.FROM_FILE_FORMAT)
                {
                    var particularFormatMenu = bufermanHost.CreateMenuItem(format);

                    var formatData = buferContextMenuState.Bufer.ViewModel.Clip.GetData(format);

                    if (formatData is Stream)
                    {
                        particularFormatMenu.Text += $" ({Resource.StreamFormat})";
                    }
                    else if (formatData is string)
                    {
                        particularFormatMenu.Text += $" ({Resource.TextFormat})";
                    }

                    if (formatData == null)
                    {
                        particularFormatMenu.Enabled = false;
                    }
                    else
                    {
                        var data = formatData.ToString();
                        if (data.Length > 2300)
                        {
                            data = $"{data.Substring(0, 2000)}{Environment.NewLine}{Environment.NewLine}...";
                        }

                        particularFormatMenu.AddOnClickHandler((object sender, EventArgs args) =>
                        {
                            bufermanHost.UserInteraction.ShowPopup(data, format);
                        });
                    }
                    formatsMenuItems.Add(particularFormatMenu);
                }
            }

            var formatsMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuFormats + $" ({formatsMenuItems.Count})");
            foreach (var formatMenuItem in formatsMenuItems)
            {
                formatsMenuItem.AddMenuItem(formatMenuItem);
            }

            menuItems.Add(formatsMenuItem);
            buferContextMenuState.DeleteBuferMenuItem = bufermanHost.CreateMenuItem(() => Resource.DeleteBuferMenuItem, (object sender, EventArgs args) => {
                buferContextMenuState.RemoveBufer();
            });
            menuItems.Add(buferContextMenuState.DeleteBuferMenuItem);

            buferContextMenuState.PasteMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuPaste);

            menuItems.Add(buferContextMenuState.PasteMenuItem);

            buferContextMenuState.PlaceInBuferMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuPlaceInBufer, (object sender, EventArgs e) =>
            {
                this._clipboardWrapper.SetDataObject(buferContextMenuState.Bufer.ViewModel.Clip);
            });
            buferContextMenuState.PlaceInBuferMenuItem.ShortCut = Shortcut.CtrlC;

            if (bufer.ViewModel.IsChangeTextAvailable)
            {
                buferContextMenuState.PasteMenuItem.AddMenuItem(bufermanHost.CreateMenuItem(() => Resource.MenuPasteAsIs + $" {new String('\t', 4)} Enter", (object sender, EventArgs ars) =>
                {
                    new KeyboardEmulator().PressEnter();
                }));

                if (formats.Length != 3 || ClipboardFormats.TextFormats.Any(tf => !formats.Contains(tf)))
                {
                    var pasteAsTextMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuPasteAsText, (object sender, EventArgs args) =>
                    {
                        var textDataObject = new DataObject();
                        textDataObject.SetText(buferViewModel.OriginBuferTitle);

                        var oldClip = buferViewModel.Clip;
                        buferViewModel.Clip = textDataObject;
                        var textBuferSelectionHandler = this._buferSelectionHandlerFactory.CreateHandler(bufer, bufermanHost);
                        textBuferSelectionHandler.DoOnClipSelection(sender, args);
                        buferViewModel.Clip = oldClip;
                    });
                    buferContextMenuState.PasteMenuItem.AddMenuItem(pasteAsTextMenuItem);

                    pasteAsTextMenuItem.ShortCut = Shortcut.CtrlA;
                }

                buferContextMenuState.PasteMenuItem.AddMenuItem(bufermanHost.CreateMenuItem(() => Resource.MenuCharByChar, (object sender, EventArgs args) =>
                {
                    bufermanHost.HideWindow();
                    new KeyboardEmulator().TypeText(buferContextMenuState.Bufer.ViewModel.OriginBuferTitle);
                }));

                menuItems.Add(bufermanHost.CreateMenuSeparatorItem());

                var editTextMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuEdit);
                new EditBuferTextMenuItem(editTextMenuItem, buferContextMenuState, bufermanHost, this._settings, this._clipboardWrapper);
                menuItems.Add(editTextMenuItem);

                var returnTextToInitialMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuReturn);
                new ReturnToInitialTextMenuItem(returnTextToInitialMenuItem, buferContextMenuState.Bufer, bufermanHost);
                buferContextMenuState.ReturnTextToInitialMenuItem = returnTextToInitialMenuItem;
                var changeTextMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuChange);
                var ctmi = new ChangeTextMenuItem(changeTextMenuItem, buferContextMenuState.Bufer, bufermanHost, this._settings);
                if (!string.IsNullOrWhiteSpace(buferViewModel.Alias))
                {
                    ctmi.TryChangeText(buferViewModel.Alias);
                }
                ctmi.TextChanged += (object sender, TextChangedEventArgs e) =>
                {
                    buferContextMenuState.ReturnTextToInitialMenuItem.Enabled = !e.IsOriginText;
                };
                buferContextMenuState.ChangeTextMenuItem = changeTextMenuItem;
                menuItems.Add(buferContextMenuState.ChangeTextMenuItem);
                menuItems.Add(buferContextMenuState.ReturnTextToInitialMenuItem);

                buferContextMenuState.AddToFileMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuAddToFile);

                if (formats.Contains(ClipboardFormats.FROM_FILE_FORMAT))
                {
                    buferContextMenuState.MarkMenuItemAsAddedToFile();
                }
                else
                {
                    var addToDefaultFileMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuAddToDefaultFile, (object sender, EventArgs args) =>
                    {
                        var bufersStorage = this._bufersStorageFactory.CreateStorageByFileExtension(
                            this._fileStorage.CombinePaths(
                                this._fileStorage.DataDirectory,
                                this._settings.DefaultBufersFileName));

                        var buferItem = buferContextMenuState.Bufer.ViewModel.ToModel();

                        bufersStorage.SaveBufer(buferItem);

                        buferContextMenuState.MarkMenuItemAsAddedToFile();
                    });

                    addToDefaultFileMenuItem.ShortCut = Shortcut.CtrlF;
                    buferContextMenuState.AddToFileMenuItem.AddMenuItem(addToDefaultFileMenuItem);

                    var addToFileMenuItem = bufermanHost.CreateMenuItem(() => Resource.MenuAddToSelectedFile, (object sender, EventArgs args) =>
                    {
                        var buferItem = buferContextMenuState.Bufer.ViewModel.ToModel();

                        this._userFileSelector.TrySelectBufersStorage(storage => storage.SaveBufer(buferItem));
                    });

                    buferContextMenuState.AddToFileMenuItem.AddSeparator();
                    buferContextMenuState.AddToFileMenuItem.AddMenuItem(addToFileMenuItem);
                }
                menuItems.Add(buferContextMenuState.AddToFileMenuItem);

                var loginCredentialsMenuItem = bufermanHost.CreateMenuItem(() => Resource.CreateCredsMenuItem);
                var clcmi = new CreateLoginCredentialsMenuItem(loginCredentialsMenuItem, buferContextMenuState.Bufer, bufermanHost);
                clcmi.LoginCreated += (object sender, CreateLoginCredentialsEventArgs e) =>
                {
                    buferContextMenuState.PasteMenuItem.SetTextFunction(() => $"{Resource.LoginWith} {new String('\t', 2)} Enter");
                    buferContextMenuState.PasteMenuItem.TextRefresh();

                    buferContextMenuState.ReturnTextToInitialMenuItem.Enabled = false;
                    buferContextMenuState.PlaceInBuferMenuItem.Enabled = false;
                    buferContextMenuState.ChangeTextMenuItem.Enabled = false;
                    buferViewModel.Clip.SetData(ClipboardFormats.PASSWORD_FORMAT, e.Password);
                    if (!buferViewModel.Pinned)
                    {
                        buferContextMenuState.TryTogglePinBufer(sender, e);
                    }

                    buferContextMenuState.RemoveBuferSelectionHandler();
                };
                buferContextMenuState.CreateLoginDataMenuItem = loginCredentialsMenuItem;
                menuItems.Add(buferContextMenuState.CreateLoginDataMenuItem);

                buferContextMenuState.PasteMenuItem.AddSeparator();

                buferContextMenuState.PasteMenuItem.AddMenuItem(buferContextMenuState.PlaceInBuferMenuItem);
            }
            else
            {
                buferContextMenuState.PasteMenuItem.Text += $" {new String('\t', 4)} Enter";
                buferContextMenuState.PasteMenuItem.AddOnClickHandler((object sender, EventArgs ars) =>
                {
                    new KeyboardEmulator().PressEnter();
                });

                menuItems.Add(buferContextMenuState.PlaceInBuferMenuItem);
            }

            if (buferTypeMenuGenerator != null)
            {
                menuItems = buferTypeMenuGenerator.Generate(menuItems, bufermanHost);
            }

            menuItems.Add(bufermanHost.CreateMenuSeparatorItem());
            var createdAtMenuItem = bufermanHost.CreateMenuItem(() => string.Format(Resource.MenuCopyingTime, buferViewModel.CreatedAt));
            createdAtMenuItem.Enabled = false;
            menuItems.Add(createdAtMenuItem);

            return menuItems;
        }
    }
}