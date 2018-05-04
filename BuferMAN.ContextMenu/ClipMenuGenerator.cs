﻿using BuferMAN.Clipboard;
using BuferMAN.Form;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using ClipboardViewerForm.ClipMenu.Items;
using System;
using System.IO;
using System.Windows.Forms;
using SystemWindowsFormsContextMenu = System.Windows.Forms.ContextMenu;
using Windows;
using BuferMAN.Menu;
using BuferMAN.ContextMenu.Properties;

namespace BuferMAN.ContextMenu
{
    public class ClipMenuGenerator : IClipMenuGenerator
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IBuferSelectionHandler _buferSelectionHandler;
        private readonly IProgramSettings _settings;
        private IDataObject _dataObject;
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
        private IClipboardWrapper _clipboardWrapper;

        public ClipMenuGenerator(IClipboardBuferService clipboardBuferService, BuferSelectionHandler buferSelectionHandler, IProgramSettings settings, IClipboardWrapper clipboardWrapper)
        {
            this._clipboardBuferService = clipboardBuferService;
            this._buferSelectionHandler = buferSelectionHandler;
            this._settings = settings;
            this._clipboardWrapper = clipboardWrapper;
        }

        public SystemWindowsFormsContextMenu GenerateContextMenu(IDataObject dataObject, Button button, ToolTip mouseOverTooltip, bool isChangeTextAvailable)
        {
            this._dataObject = dataObject;
            this._button = button;
            this._originBuferText = button.Text;
            this._mouseOverTooltip = mouseOverTooltip;

            var contextMenu = new SystemWindowsFormsContextMenu();

            this._markAsPersistentMenuItem = new MenuItem(Resource.MenuPersistent, this._MarkAsPersistent, Shortcut.CtrlS);
            contextMenu.MenuItems.Add(this._markAsPersistentMenuItem);
            this._placeInBuferMenuItem = new PlaceInBuferMenuItem(this._clipboardWrapper, this._dataObject);
            contextMenu.MenuItems.Add(this._placeInBuferMenuItem);

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
            contextMenu.MenuItems.Add(new DeleteClipMenuItem(this._clipboardBuferService, dataObject, button));

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
                changeTextMenuItem.TextChanged += this._ChangeTextMenuItem_TextChanged;
                this._changeTextMenuItem = changeTextMenuItem;
                contextMenu.MenuItems.Add(this._changeTextMenuItem);

                this._addToFileMenuItem = new MenuItem(Resource.MenuAddToFile, (object sender, EventArgs args) =>
                {
                    using (var sw = new StreamWriter(new FileStream(this._settings.DefaultBufersFileName, FileMode.Append, FileAccess.Write)))
                    {
                        sw.WriteLine(this._originBuferText);
                    }
                    this._addToFileMenuItem.Text = Resource.MenuAddedToFile;
                    this._addToFileMenuItem.Enabled = false;
                }, Shortcut.CtrlF);
                contextMenu.MenuItems.Add(this._addToFileMenuItem);

                var loginCredentialsMenuItem = new CreateLoginCredentialsMenuItem(this._button, this._originBuferText, this._mouseOverTooltip);
                loginCredentialsMenuItem.LoginCreated += this._LoginCredentialsMenuItem_LoginCreated;
                this._createLoginDataMenuItem = loginCredentialsMenuItem;
                contextMenu.MenuItems.Add(this._createLoginDataMenuItem);
            }

            return contextMenu;
        }

        private void _LoginCredentialsMenuItem_LoginCreated(object sender, CreateLoginCredentialsEventArgs e)
        {
            this._pasteMenuItem.Text = $"{Resource.LoginWith} {new String('\t', 2)} Enter";

            this._returnTextToInitialMenuItem.Enabled = false;
            this._placeInBuferMenuItem.Enabled = false;
            this._changeTextMenuItem.Enabled = false;
            this._dataObject.SetData(ClipboardFormats.PASSWORD_FORMAT, e.Password);
            this._MarkAsPersistent(sender, e);
            this._button.Click -= this._buferSelectionHandler.DoOnClipSelection;
        }

        private void _ChangeTextMenuItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            this._returnTextToInitialMenuItem.Enabled = !e.IsOriginText;
        }

        private void _MarkAsPersistent(object sender, EventArgs e)
        {
            if (this._clipboardBuferService.MarkClipAsPersistent(this._dataObject))
            {
                this._markAsPersistentMenuItem.Enabled = false;
                WindowLevelContext.Current.RerenderBufers();
            } else
            {
                MessageBox.Show("This bufer cannot be marked as persistent.");
            }
        }
    }
}