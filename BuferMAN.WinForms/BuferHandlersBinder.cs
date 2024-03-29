﻿using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Infrastructure.Files;
using BuferMAN.Clipboard;
using System.Collections.Generic;
using BuferMAN.Infrastructure.Plugins;

namespace BuferMAN.WinForms
{
    internal class BuferHandlersBinder : IBuferHandlersBinder
    {
        private readonly IClipboardBuferService _clipboardBuferService;
        private readonly IBuferContextMenuGenerator _buferContextMenuGenerator;
        private readonly IBuferSelectionHandlerFactory _buferSelectionHandlerFactory;
        private readonly IFileStorage _fileStorage;
        private readonly IProgramSettingsGetter _settingsGetter;
        private readonly IProgramSettingsSetter _settingsSetter;
        private readonly IEnumerable<IBufermanPlugin> _plugins;

        public BuferHandlersBinder(
            IBuferContextMenuGenerator buferContextMenuGenerator,
            IBuferSelectionHandlerFactory buferSelectionHandlerFactory,
            IFileStorage fileStorage,
            IProgramSettingsGetter settingsGetter,
            IProgramSettingsSetter settingsSetter,
            IEnumerable<IBufermanPlugin> plugins,
            IClipboardBuferService clipboardBuferService)
        {
            this._buferContextMenuGenerator = buferContextMenuGenerator;
            this._buferSelectionHandlerFactory = buferSelectionHandlerFactory;
            this._fileStorage = fileStorage;
            this._settingsGetter = settingsGetter;
            this._settingsSetter = settingsSetter;
            this._plugins = plugins;
            this._clipboardBuferService = clipboardBuferService;
        }

        public void Bind(IBufer bufer, IBufermanHost bufermanHost)
        {
            new BuferHandlersWrapper(this._clipboardBuferService, this._buferContextMenuGenerator, this._buferSelectionHandlerFactory, this._fileStorage, bufermanHost, this._settingsGetter, this._settingsSetter, this._plugins, bufer);
        }
    }
}
